using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Scada.TemperatureMonitoring
{
    public class TemperatureRepository
    {
        public async Task<TemperatureConfigsModel> GetConfigAsync()
        {
            using (var db = new ApplicationDbContext())
            {
                var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                if (ft10 != null && !string.IsNullOrEmpty(ft10.C000))
                {
                    var configWrapper = JsonConvert.DeserializeObject<TemperatureConfigsModel>(ft10.C000) 
                        ?? new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() };
                    if (configWrapper.LocationsConfig == null) configWrapper.LocationsConfig = new List<TemperatureLocationModel>();
                    return configWrapper;
                }
                return new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() };
            }
        }

        public async Task UpdateOffsetAsync(int? locationId, double offset)
        {
            using (var db = new ApplicationDbContext())
            {
                var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                if (ft10 != null)
                {
                    var wrapper = JsonConvert.DeserializeObject<TemperatureConfigsModel>(ft10.C000) 
                        ?? new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() };
                    var loc = wrapper.LocationsConfig?.FirstOrDefault(x => x.Id == locationId);
                    if (loc != null)
                    {
                        loc.Offset = offset;
                        ft10.C000 = JsonConvert.SerializeObject(wrapper);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task SaveRealtimeDataAsync(List<TemperatureRealtimeModel> realtimeModels)
        {
            using (var db = new ApplicationDbContext())
            {
                var now = DateTime.Now;
                var existingFt11 = await db.FT11_TemperatureRealtimes.FirstOrDefaultAsync();
                if (existingFt11 != null)
                {
                    existingFt11.C000 = JsonConvert.SerializeObject(realtimeModels);
                    existingFt11.CreatedAt = now;
                }
                else
                {
                    db.FT11_TemperatureRealtimes.Add(new FT11_TemperatureRealtime
                    {
                        Id = Guid.NewGuid(),
                        C000 = JsonConvert.SerializeObject(realtimeModels),
                        CreatedAt = now
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task SaveDataLogsAsync(List<FT12_TemperatureDatalog> datalogs)
        {
            using (var db = new ApplicationDbContext())
            {
                db.FT12_TemperatureDatalogs.AddRange(datalogs);
                await db.SaveChangesAsync();
            }
        }

        public async Task<FT13_TemperatureAlarmLog> GetActiveAlarmAsync(int? locationId)
        {
            using (var db = new ApplicationDbContext())
            {
                return await db.FT13_TemperatureAlarmLogs
                    .Where(x => x.LocationId == locationId && x.EndedAt == null)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task CreateAlarmAsync(FT13_TemperatureAlarmLog alarm)
        {
            using (var db = new ApplicationDbContext())
            {
                db.FT13_TemperatureAlarmLogs.Add(alarm);
                await db.SaveChangesAsync();
            }
        }

        public async Task EndAlarmAsync(Guid alarmId, double pvNormal, DateTime endTime)
        {
            using (var db = new ApplicationDbContext())
            {
                var alarm = await db.FT13_TemperatureAlarmLogs.FindAsync(alarmId);
                if (alarm != null)
                {
                    alarm.EndedAt = endTime;
                    alarm.PV_Normal = pvNormal;
                    alarm.UpdateddAt = DateTime.Now;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<TemperatureConfigsModel> CheckAndResetTriggerUpdateAsync()
        {
            using (var db = new ApplicationDbContext())
            {
                var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                if (ft10 != null && !string.IsNullOrEmpty(ft10.C000))
                {
                    var wrapper = JsonConvert.DeserializeObject<TemperatureConfigsModel>(ft10.C000) 
                        ?? new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() };
                    
                    if (wrapper.LocationsConfig != null && wrapper.LocationsConfig.Any(x => x.TriggerUpdate))
                    {
                        foreach (var loc in wrapper.LocationsConfig)
                        {
                            loc.TriggerUpdate = false;
                        }
                        ft10.C000 = JsonConvert.SerializeObject(wrapper);
                        await db.SaveChangesAsync();
                        return wrapper; // return new config
                    }
                }
                return null;
            }
        }
    }
}
