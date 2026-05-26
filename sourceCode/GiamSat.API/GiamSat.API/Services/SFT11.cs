using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT11 : ISFT11
    {
        readonly ApplicationDbContext _context;

        public SFT11(ApplicationDbContext context)
        {
            _context = context;
            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<TemperatureRealtimeModel>>> GetRealtime()
        {
            try
            {
                var ft11 = await _context.FT11_TemperatureRealtimes.FirstOrDefaultAsync();
                if (ft11 == null || string.IsNullOrEmpty(ft11.C000))
                    return await Result<List<TemperatureRealtimeModel>>.SuccessAsync(new List<TemperatureRealtimeModel>());

                var data = JsonConvert.DeserializeObject<List<TemperatureRealtimeModel>>(ft11.C000);
                return await Result<List<TemperatureRealtimeModel>>.SuccessAsync(
                    data ?? new List<TemperatureRealtimeModel>());
            }
            catch (Exception ex)
            {
                return await Result<List<TemperatureRealtimeModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<FT13_TemperatureAlarmLog>>> GetAlarmLogs([Query] DateTime fromDate, [Query] DateTime toDate)
        {
            try
            {
                var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
                var logs = await _context.FT13_TemperatureAlarmLogs
                    .Where(x => x.CreatedAt >= fromDate.Date && x.CreatedAt <= endOfDay)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
                return await Result<List<FT13_TemperatureAlarmLog>>.SuccessAsync(logs);
            }
            catch (Exception ex)
            {
                return await Result<List<FT13_TemperatureAlarmLog>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<FT12_TemperatureDatalog>>> GetDataLogs([Query] DateTime fromDate, [Query] DateTime toDate)
        {
            try
            {
                var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
                var logs = await _context.FT12_TemperatureDatalogs
                    .Where(x => x.CreatedAt >= fromDate.Date && x.CreatedAt <= endOfDay)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
                return await Result<List<FT12_TemperatureDatalog>>.SuccessAsync(logs);
            }
            catch (Exception ex)
            {
                return await Result<List<FT12_TemperatureDatalog>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> SyncRealtime([Body] List<TemperatureRealtimeModel> reqData)
        {
            try
            {
                if (reqData == null || reqData.Count == 0)
                    return await Result<bool>.SuccessAsync(true);

                var now = DateTime.Now;

                var ft11 = await _context.FT11_TemperatureRealtimes.FirstOrDefaultAsync();
                var jsonString = JsonConvert.SerializeObject(reqData);

                if (ft11 == null)
                {
                    ft11 = new FT11_TemperatureRealtime
                    {
                        Id = Guid.NewGuid(),
                        C000 = jsonString,
                        CreatedAt = now
                    };
                    await _context.FT11_TemperatureRealtimes.AddAsync(ft11);
                }
                else
                {
                    ft11.C000 = jsonString;
                    _context.FT11_TemperatureRealtimes.Update(ft11);
                }

                var locationIds = reqData.Select(x => x.Id).ToList();
                var openAlarms = await _context.FT13_TemperatureAlarmLogs
                    .Where(x => x.UpdateddAt == null && locationIds.Contains(x.LocationId ?? -1))
                    .ToListAsync();

                foreach (var model in reqData)
                {
                    bool isAlarm = model.PV > model.Config.HightLevel || model.PV < model.Config.LowLevel;
                    var existingAlarm = openAlarms.FirstOrDefault(x => x.LocationId == model.Id);

                    if (isAlarm)
                    {
                        if (existingAlarm == null)
                        {
                            await _context.FT13_TemperatureAlarmLogs.AddAsync(new FT13_TemperatureAlarmLog
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = now,
                                CreatedMachine = "Connector",
                                LocationId = model.Id,
                                LocationName = model.Name,
                                Path = model.Path,
                                PV_Alarm = model.PV,
                                SV_High = model.Config.HightLevel,
                                SV_Low = model.Config.LowLevel,
                                Description = model.PV > model.Config.HightLevel
                                    ? $"Nhiệt độ cảnh báo cao: {model.PV}°C (Ngưỡng {model.Config.HightLevel}°C)"
                                    : $"Nhiệt độ cảnh báo thấp: {model.PV}°C (Ngưỡng {model.Config.LowLevel}°C)"
                            });
                        }
                    }
                    else if (existingAlarm != null)
                    {
                        existingAlarm.PV_Normal = model.PV;
                        existingAlarm.UpdateddAt = now;
                        existingAlarm.Description += $" | Đã khôi phục PV = {model.PV}°C";
                        _context.FT13_TemperatureAlarmLogs.Update(existingAlarm);
                    }
                }

                await _context.SaveChangesAsync();
                return await Result<bool>.SuccessAsync(true);
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> SyncDatalog([Body] List<TemperatureRealtimeModel> reqData)
        {
            try
            {
                if (reqData == null || reqData.Count == 0)
                    return await Result<bool>.SuccessAsync(true);

                var now = DateTime.Now;
                var datalogs = reqData.Select(model => new FT12_TemperatureDatalog
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = now,
                    CreatedMachine = "Connector",
                    LocationId = model.Id,
                    LocationName = model.Name,
                    PV = model.PV
                }).ToList();

                await _context.FT12_TemperatureDatalogs.AddRangeAsync(datalogs);
                await _context.SaveChangesAsync();
                return await Result<bool>.SuccessAsync(true);
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync(ex.Message);
            }
        }
    }
}
