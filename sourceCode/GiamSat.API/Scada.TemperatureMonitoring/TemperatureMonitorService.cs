using EasyScada.Core;
using EasyScada.Winforms.Controls;
using EasyDriverConnector = EasyScada.Winforms.Controls.EasyDriverConnector;
using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Scada.TemperatureMonitoring
{
    /// <summary>
    /// Cache lưu giá trị PV và Quality mới nhất của từng location (path).
    /// Được cập nhật ngay lập tức từ TagChange events (không delay).
    /// Các task định kỳ chỉ đọc cache này thay vì gọi GetTag() lại.
    /// </summary>
    internal class TagCache
    {
        public double PV { get; set; }
        public Quality Quality { get; set; } = Quality.Bad;
        public bool PlcConnected => Quality == Quality.Good;
    }

    public class TemperatureMonitorService
    {
        private readonly TemperatureRepository _repository;
        private readonly EasyDriverConnector _driver;
        private TemperatureConfigsModel _config;

        // === CACHE: TagChange → cập nhật đây, các task chỉ đọc từ đây ===
        // Key: loc.Path (ví dụ "Local Station/Channel1/Oven1")
        private readonly Dictionary<string, TagCache> _tagCache
            = new Dictionary<string, TagCache>(StringComparer.OrdinalIgnoreCase);

        // === ALARM STATE CACHE: track trạng thái alarm trong memory ===
        // Key: locationId, Value: Id của alarm record đang active (null = không có alarm)
        private readonly Dictionary<int, Guid?> _activeAlarmIds
            = new Dictionary<int, Guid?>();

        public event Action<string, string, Quality> OnDeviceUpdated;
        public event Action<TemperatureConfigsModel> OnConfigUpdated;
        public event Action<string, Color> OnAlarmBlink;

        public TemperatureMonitorService(TemperatureRepository repository, EasyDriverConnector driver)
        {
            _repository = repository;
            _driver = driver;
        }

        /// <summary>
        /// Đăng ký TagChange events cho tất cả location trong config.
        /// Đồng thời khởi tạo cache cho từng location.
        /// </summary>
        public void SetConfig(TemperatureConfigsModel config)
        {
            _config = config;
            if (_driver.IsStarted && _config.LocationsConfig != null)
            {
                foreach (var loc in _config.LocationsConfig)
                {
                    // Khởi tạo cache entry nếu chưa có
                    if (!_tagCache.ContainsKey(loc.Path))
                        _tagCache[loc.Path] = new TagCache();

                    // Khởi tạo alarm state cache nếu chưa có
                    if (loc.Id.HasValue && !_activeAlarmIds.ContainsKey(loc.Id.Value))
                        _activeAlarmIds[loc.Id.Value] = null;

                    var tagPv = _driver.GetTag($"{loc.Path}/PV");
                    if (tagPv != null)
                    {
                        tagPv.ValueChanged -= TagPv_ValueChanged;
                        tagPv.ValueChanged += TagPv_ValueChanged;

                        tagPv.QualityChanged -= TagPv_QualityChanged;
                        tagPv.QualityChanged += TagPv_QualityChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Đọc giá trị hiện tại của tất cả tag lần đầu (sau khi driver started),
        /// cập nhật vào cache và fire event để UI hiển thị ngay.
        /// </summary>
        public void TriggerFirstTimeFetch()
        {
            if (_driver.IsStarted && _config?.LocationsConfig != null)
            {
                foreach (var loc in _config.LocationsConfig)
                {
                    var tagPv = _driver.GetTag($"{loc.Path}/PV");
                    if (tagPv != null)
                    {
                        // Cập nhật cache ngay lần đầu
                        UpdateCache(loc.Path, tagPv.Value, tagPv.Quality);
                        OnDeviceUpdated?.Invoke(loc.Path, tagPv.Value, tagPv.Quality);
                    }
                }
            }
        }

        // =====================================================================
        // TAG CHANGE HANDLERS — CHỈ CẬP NHẬT CACHE, không gọi DB hay logic phức tạp
        // =====================================================================

        private void TagPv_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                string pathPrefix = tag.Path.Substring(0, tag.Path.LastIndexOf('/'));
                // Cập nhật cache
                UpdateCache(pathPrefix, e.NewValue, tag.Quality);
                // Thông báo UI
                OnDeviceUpdated?.Invoke(pathPrefix, e.NewValue, tag.Quality);
            }
        }

        private void TagPv_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                string pathPrefix = tag.Path.Substring(0, tag.Path.LastIndexOf('/'));
                // Cập nhật cache
                UpdateCache(pathPrefix, tag.Value, e.NewQuality);
                // Thông báo UI
                OnDeviceUpdated?.Invoke(pathPrefix, tag.Value, e.NewQuality);
            }
        }

        /// <summary>
        /// Cập nhật giá trị PV và Quality vào cache theo path.
        /// Thread-safe: TagChange có thể fire từ thread khác.
        /// </summary>
        private void UpdateCache(string path, string valueStr, Quality quality)
        {
            if (!_tagCache.TryGetValue(path, out var cache))
            {
                cache = new TagCache();
                _tagCache[path] = cache;
            }
            double.TryParse(valueStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double pv);
            cache.PV = pv;
            cache.Quality = quality;
        }

        // =====================================================================
        // BACKGROUND TASKS
        // =====================================================================

        public async Task StartTasksAsync(CancellationToken token)
        {
            // Tải trạng thái alarm hiện tại từ DB một lần khi khởi động
            await InitAlarmStateFromDbAsync();

            var t1 = Task.Run(() => TaskRealtimeLogAsync(token));
            var t2 = Task.Run(() => TaskDataLogAsync(token));
            var t3 = Task.Run(() => TaskAlarmMonitorAsync(token));
            var t4 = Task.Run(() => TaskCheckConfigChangesAsync(token));

            await Task.WhenAll(t1, t2, t3, t4);
        }

        /// <summary>
        /// Đọc DB 1 lần khi khởi động để nạp trạng thái alarm đang active vào memory.
        /// Sau đó TaskAlarmMonitorAsync chỉ cần check cache, không query DB mỗi giây.
        /// </summary>
        private async Task InitAlarmStateFromDbAsync()
        {
            if (_config?.LocationsConfig == null) return;
            foreach (var config in _config.LocationsConfig)
            {
                if (!config.Id.HasValue) continue;
                try
                {
                    var existingAlarm = await _repository.GetActiveAlarmAsync(config.Id);
                    _activeAlarmIds[config.Id.Value] = existingAlarm?.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading alarm state for {config.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Task lưu realtime định kỳ vào FT11.
        /// Đọc từ cache (_tagCache), KHÔNG gọi GetTag().
        /// </summary>
        private async Task TaskRealtimeLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_config?.LocationsConfig != null && _config.LocationsConfig.Count > 0)
                    {
                        bool easyConnected = _driver != null
                            && _driver.IsStarted
                            && _driver.ConnectionStatus == ConnectionStatus.Connected;

                        var realtimeModels = new List<TemperatureRealtimeModel>();

                        foreach (var config in _config.LocationsConfig)
                        {
                            // ✅ Đọc từ cache — không gọi GetTag()
                            _tagCache.TryGetValue(config.Path, out var cache);

                            realtimeModels.Add(new TemperatureRealtimeModel
                            {
                                Id = config.Id ?? 0,
                                Name = config.Name,
                                Path = config.Path,
                                Status = easyConnected && (cache?.PlcConnected ?? false),
                                ConnectionStatus = easyConnected,
                                PV = cache?.PV ?? 0,
                                Config = config
                            });
                        }

                        await _repository.SaveRealtimeDataAsync(realtimeModels);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskRealtimeLogAsync: " + ex.Message);
                }

                int delay = (int)(_config?.IntervalRealtime ?? 10000);
                if (delay <= 0) delay = 10000;
                await Task.Delay(delay, token);
            }
        }

        /// <summary>
        /// Task log datalog định kỳ vào FT12.
        /// Đọc từ cache (_tagCache), KHÔNG gọi GetTag().
        /// </summary>
        private async Task TaskDataLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_config?.LocationsConfig != null && _config.LocationsConfig.Count > 0)
                    {
                        var datalogs = new List<FT12_TemperatureDatalog>();
                        var now = DateTime.Now;

                        foreach (var config in _config.LocationsConfig)
                        {
                            // ✅ Đọc từ cache — không gọi GetTag()
                            _tagCache.TryGetValue(config.Path, out var cache);
                            double pv = cache?.PV ?? 0;

                            datalogs.Add(new FT12_TemperatureDatalog
                            {
                                Id = Guid.NewGuid(),
                                LocationId = config.Id,
                                LocationName = config.Name,
                                PV = pv,
                                CreatedAt = now
                            });
                        }

                        if (datalogs.Count > 0)
                        {
                            await _repository.SaveDataLogsAsync(datalogs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskDataLogAsync: " + ex.Message);
                }

                int delay = (int)(_config?.IntervalDataLog ?? 300000);
                if (delay <= 0) delay = 300000;
                await Task.Delay(delay, token);
            }
        }

        /// <summary>
        /// Task giám sát alarm và nhấp nháy UI.
        /// Đọc PV từ cache (_tagCache) — KHÔNG gọi GetTag().
        /// Trạng thái alarm (active/inactive) được track trong _activeAlarmIds (in-memory)
        /// → chỉ query/write DB khi alarm state thực sự thay đổi (không query mỗi giây).
        /// </summary>
        private async Task TaskAlarmMonitorAsync(CancellationToken token)
        {
            bool isBlinkOn = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_driver.IsStarted
                        && _config?.LocationsConfig != null
                        && _config.LocationsConfig.Count > 0)
                    {
                        isBlinkOn = !isBlinkOn;
                        var now = DateTime.Now;

                        foreach (var config in _config.LocationsConfig)
                        {
                            if (!config.Id.HasValue) continue;
                            int locationId = config.Id.Value;

                            // ✅ Đọc từ cache — không gọi GetTag()
                            _tagCache.TryGetValue(config.Path, out var cache);
                            double pv = cache?.PV ?? 0;
                            bool plcConnected = cache?.PlcConnected ?? false;

                            // Nếu PLC mất kết nối thì bỏ qua alarm check
                            if (!plcConnected) continue;

                            bool isAlarm = pv > config.HightLevel || pv < config.LowLevel;

                            // Lấy trạng thái alarm hiện tại từ memory (không query DB)
                            _activeAlarmIds.TryGetValue(locationId, out Guid? activeAlarmId);
                            bool hasActiveAlarm = activeAlarmId.HasValue;

                            if (isAlarm)
                            {
                                // Nhấp nháy UI
                                OnAlarmBlink?.Invoke(config.Path, isBlinkOn ? Color.Red : Color.White);

                                // Chỉ tạo alarm record mới nếu chưa có alarm active
                                if (!hasActiveAlarm)
                                {
                                    string desc = pv > config.HightLevel
                                        ? "Quá nhiệt độ cao"
                                        : "Dưới nhiệt độ thấp";

                                    var newAlarm = new FT13_TemperatureAlarmLog
                                    {
                                        Id = Guid.NewGuid(),
                                        LocationId = config.Id,
                                        LocationName = config.Name,
                                        Path = config.Path,
                                        PV_Alarm = pv,
                                        SV_High = config.HightLevel,
                                        SV_Low = config.LowLevel,
                                        Description = desc,
                                        CreatedAt = now,
                                        CreatedMachine = Environment.MachineName
                                    };

                                    await _repository.CreateAlarmAsync(newAlarm);

                                    // Cập nhật cache trạng thái alarm
                                    _activeAlarmIds[locationId] = newAlarm.Id;
                                }
                            }
                            else
                            {
                                // Tắt nhấp nháy
                                OnAlarmBlink?.Invoke(config.Path, Color.White);

                                // Chỉ kết thúc alarm nếu đang có alarm active
                                if (hasActiveAlarm)
                                {
                                    await _repository.EndAlarmAsync(activeAlarmId.Value, pv, now);

                                    // Xóa cache trạng thái alarm
                                    _activeAlarmIds[locationId] = null;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskAlarmMonitorAsync: " + ex.Message);
                }

                int delay = (int)(_config?.TimeBlinkAlarm ?? 1000);
                if (delay <= 0) delay = 1000;
                await Task.Delay(delay, token);
            }
        }

        /// <summary>
        /// Task kiểm tra thay đổi config từ DB (mỗi 5 giây).
        /// Khi có config mới thì re-register TagChange events và ghi Offset xuống PLC.
        /// </summary>
        private async Task TaskCheckConfigChangesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var newConfig = await _repository.CheckAndResetTriggerUpdateAsync();
                    if (newConfig != null)
                    {
                        SetConfig(newConfig);
                        OnConfigUpdated?.Invoke(newConfig);

                        if (_driver.IsStarted && newConfig.LocationsConfig != null)
                        {
                            foreach (var loc in newConfig.LocationsConfig)
                            {
                                var tag = _driver.GetTag($"{loc.Path}/Offset");
                                if (tag != null)
                                {
                                    await tag.WriteAsync(
                                        loc.Offset.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        WritePiority.High);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskCheckConfigChangesAsync: " + ex.Message);
                }

                await Task.Delay(5000, token);
            }
        }
    }
}
