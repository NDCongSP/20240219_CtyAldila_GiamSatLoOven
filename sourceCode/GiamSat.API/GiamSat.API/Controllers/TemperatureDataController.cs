using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TemperatureDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("SyncRealtime")]
        public async Task<IActionResult> SyncRealtime([FromBody] List<TemperatureRealtimeModel> reqData)
        {
            try
            {
                if (reqData == null || reqData.Count == 0) return Ok();

                var now = DateTime.Now;

                // 1. Update FT11 (Realtime Data)
                var ft11 = await _context.FT11_TemperatureRealtimes.FirstOrDefaultAsync();
                var jsonString = JsonConvert.SerializeObject(reqData);

                if (ft11 == null)
                {
                    ft11 = new FT11_TemperatureRealtime
                    {
                        Id = Guid.NewGuid(),
                        C001_Data = jsonString,
                        CreatedAt = now
                    };
                    await _context.FT11_TemperatureRealtimes.AddAsync(ft11);
                }
                else
                {
                    ft11.C001_Data = jsonString;
                    _context.FT11_TemperatureRealtimes.Update(ft11);
                }

                // 2. Alarm Logic (Dual-state)
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
                            // Start new alarm
                            var newAlarm = new FT13_TemperatureAlarmLog
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = now,
                                CreatedMachine = "Connector",
                                LocationId = model.Id,
                                LocationName = model.Name,
                                Path = model.Path,
                                PV = model.PV,
                                SV_High = model.Config.HightLevel,
                                SV_Low = model.Config.LowLevel,
                                Description = model.PV > model.Config.HightLevel ? 
                                    $"Nhiệt độ cảnh báo cao: {model.PV}°C (Ngưỡng {model.Config.HightLevel}°C)" : 
                                    $"Nhiệt độ cảnh báo thấp: {model.PV}°C (Ngưỡng {model.Config.LowLevel}°C)"
                            };
                            await _context.FT13_TemperatureAlarmLogs.AddAsync(newAlarm);
                        }
                    }
                    else
                    {
                        if (existingAlarm != null)
                        {
                            // End existing alarm
                            existingAlarm.UpdateddAt = now;
                            existingAlarm.Description += $" | Đã khôi phục PV = {model.PV}°C";
                            _context.FT13_TemperatureAlarmLogs.Update(existingAlarm);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Sync Realtime Successfully", Timestamp = now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("SyncDatalog")]
        public async Task<IActionResult> SyncDatalog([FromBody] List<TemperatureRealtimeModel> reqData)
        {
            try
            {
                if (reqData == null || reqData.Count == 0) return Ok();

                var now = DateTime.Now;
                var datalogs = new List<FT12_TemperatureDatalog>();

                foreach (var model in reqData)
                {
                    datalogs.Add(new FT12_TemperatureDatalog
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = now,
                        CreatedMachine = "Connector",
                        LocationId = model.Id,
                        LocationName = model.Name,
                        PV = model.PV
                    });
                }

                await _context.FT12_TemperatureDatalogs.AddRangeAsync(datalogs);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Sync Datalog Successfully", Inserted = datalogs.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
