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
    public class TemperatureMonitorService
    {
        private readonly TemperatureRepository _repository;
        private readonly EasyDriverConnector _driver;
        private TemperatureConfigsModel _config;

        public event Action<string, string, Quality> OnDeviceUpdated;
        public event Action<TemperatureConfigsModel> OnConfigUpdated;
        public event Action<string, Color> OnAlarmBlink;

        public TemperatureMonitorService(TemperatureRepository repository, EasyDriverConnector driver)
        {
            _repository = repository;
            _driver = driver;
        }

        public void SetConfig(TemperatureConfigsModel config)
        {
            _config = config;
            if (_driver.IsStarted && _config.LocationsConfig != null)
            {
                foreach (var loc in _config.LocationsConfig)
                {
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

        public void TriggerFirstTimeFetch()
        {
            if (_driver.IsStarted && _config.LocationsConfig != null)
            {
                foreach (var loc in _config.LocationsConfig)
                {
                    var tagPv = _driver.GetTag($"{loc.Path}/PV");
                    if (tagPv != null)
                    {
                        OnDeviceUpdated?.Invoke(loc.Path, tagPv.Value, tagPv.Quality);
                    }
                }
            }
        }

        private void TagPv_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                string pathPrefix = tag.Path.Substring(0, tag.Path.LastIndexOf('/'));
                OnDeviceUpdated?.Invoke(pathPrefix, e.NewValue, tag.Quality);
            }
        }

        private void TagPv_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                string pathPrefix = tag.Path.Substring(0, tag.Path.LastIndexOf('/'));
                OnDeviceUpdated?.Invoke(pathPrefix, tag.Value, e.NewQuality);
            }
        }

        public async Task StartTasksAsync(CancellationToken token)
        {
            var t1 = Task.Run(() => TaskRealtimeLogAsync(token));
            var t2 = Task.Run(() => TaskDataLogAsync(token));
            var t3 = Task.Run(() => TaskAlarmMonitorAsync(token));
            var t4 = Task.Run(() => TaskCheckConfigChangesAsync(token));

            await Task.WhenAll(t1, t2, t3, t4);
        }

        private async Task TaskRealtimeLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_config?.LocationsConfig != null && _config.LocationsConfig.Count > 0)
                    {
                        var realtimeModels = new List<TemperatureRealtimeModel>();
                        bool easyConnected = _driver != null && _driver.IsStarted && _driver.ConnectionStatus == ConnectionStatus.Connected;

                        foreach (var config in _config.LocationsConfig)
                        {
                            double pv = 0;
                            bool plcConnected = false;

                            if (easyConnected)
                            {
                                var tagPv = _driver.GetTag($"{config.Path}/PV");
                                if (tagPv != null)
                                {
                                    string pvValueStr = tagPv.Value;
                                    double.TryParse(pvValueStr, out pv);
                                    plcConnected = tagPv.Quality == Quality.Good;
                                }
                            }

                            realtimeModels.Add(new TemperatureRealtimeModel
                            {
                                Id = config.Id ?? 0,
                                Name = config.Name,
                                Path = config.Path,
                                Status = plcConnected,
                                ConnectionStatus = easyConnected,
                                PV = pv,
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

        private async Task TaskDataLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_driver.IsStarted && _config?.LocationsConfig != null && _config.LocationsConfig.Count > 0)
                    {
                        var datalogs = new List<FT12_TemperatureDatalog>();
                        var now = DateTime.Now;

                        foreach (var config in _config.LocationsConfig)
                        {
                            string pvValueStr = _driver.GetTag($"{config.Path}/PV")?.Value;
                            double pv = 0;
                            double.TryParse(pvValueStr, out pv);

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

        private async Task TaskAlarmMonitorAsync(CancellationToken token)
        {
            bool isBlinkOn = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_driver.IsStarted && _config?.LocationsConfig != null && _config.LocationsConfig.Count > 0)
                    {
                        isBlinkOn = !isBlinkOn;
                        var now = DateTime.Now;

                        foreach (var config in _config.LocationsConfig)
                        {
                            string pvValueStr = _driver.GetTag($"{config.Path}/PV")?.Value;
                            double pv = 0;
                            double.TryParse(pvValueStr, out pv);

                            double currentTemp = pv;
                            bool isAlarm = currentTemp > config.HightLevel || currentTemp < config.LowLevel;
                            
                            var activeAlarm = await _repository.GetActiveAlarmAsync(config.Id);

                            if (isAlarm)
                            {
                                OnAlarmBlink?.Invoke(config.Path, isBlinkOn ? Color.Red : Color.White);

                                if (activeAlarm == null)
                                {
                                    string desc = currentTemp > config.HightLevel ? "Quá nhiệt độ cao" : "Dưới nhiệt độ thấp";
                                    await _repository.CreateAlarmAsync(new FT13_TemperatureAlarmLog
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
                                    });
                                }
                            }
                            else
                            {
                                OnAlarmBlink?.Invoke(config.Path, Color.White);

                                if (activeAlarm != null)
                                {
                                    await _repository.EndAlarmAsync(activeAlarm.Id, pv, now);
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
                                    await tag.WriteAsync(loc.Offset.ToString(System.Globalization.CultureInfo.InvariantCulture), WritePiority.High);
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
