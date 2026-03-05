using GiamSat.API;
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
    public class RevoMockController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RevoMockController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate mock data với các step có startAt/endAt để test màu sắc realtime
        /// </summary>
        [HttpPost("GenerateMockData")]
        public async Task<IActionResult> GenerateMockData()
        {
            try
            {
                var now = DateTime.Now;
                var ft07 = await _context.FT07_RevoConfigs.FirstOrDefaultAsync();
                if (ft07 == null || string.IsNullOrEmpty(ft07.C000))
                {
                    return BadRequest("FT07 config not found");
                }

                var revoConfigs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);
                if (revoConfigs == null || revoConfigs.Count == 0)
                {
                    return BadRequest("No REVO configs found");
                }

                var updatedRealtimes = new List<FT08_RevoRealtime>();

                var rnd = new Random();
                var partPrefixes = new[] { "U0381B1", "P1538I1", "N0298A1", "P1799A0", "P0195A0", "M1371A1", "M1187A0", "CW183IR" };

                foreach (var config in revoConfigs)
                {
                    var steps = new List<RevoStep>();
                    var baseTime = now.AddMinutes(-60); // Base time 1h trước
                    
                    // Tạo 10 steps mô phỏng giống ảnh thực tế
                    for (int j = 1; j <= 10; j++)
                    {
                        var partPrefix = partPrefixes[rnd.Next(partPrefixes.Length)];
                        var partNumber = $"0{rnd.Next(2, 4)}10{rnd.Next(10, 60)}";
                        var speedVal = new[] { 4800, 6400, 7200, 19200 }[rnd.Next(4)];
                        var pulVal = rnd.Next(5, 30) * 800; // 4000 - 24000

                        var step = new RevoStep()
                        {
                            StepIndex = j,
                            StepName = $"{partPrefix}-{partNumber}",
                            StepConfig = $"{partPrefix}-{partNumber}|{pulVal}|{speedVal}|N",
                            Visible = true,
                            Enanble = true,
                            Speed_Hz = speedVal,
                            SoLuongXung = pulVal,
                            StartAt = null,
                            EndAt = null,
                            TotalRunTime = 0
                        };

                        // Phân bổ trạng thái:
                        // Steps 1-3: Đã chạy xong (gray) - có StartAt, EndAt, TotalRunTime
                        // Step 4: Đang chạy (green) - có StartAt, chưa có EndAt
                        // Steps 5-8: Chưa chạy (cyan blue) - chưa có StartAt
                        // Steps 9-10: Disabled (black) - Enable = false

                        if (j <= 3)
                        {
                            // Đã chạy xong → Gray
                            var startTime = baseTime.AddMinutes((j - 1) * 3);
                            var runSeconds = rnd.Next(80, 200); // 80-200 giây
                            step.StartAt = startTime;
                            step.EndAt = startTime.AddSeconds(runSeconds);
                            step.TotalRunTime = runSeconds + rnd.NextDouble() * 0.99; // e.g. 137.94s
                        }
                        else if (j == 4)
                        {
                            // Đang chạy → Green
                            step.StartAt = now.AddSeconds(-rnd.Next(10, 60));
                            step.EndAt = null; // chưa kết thúc
                            step.TotalRunTime = null;
                        }
                        else if (j >= 5 && j <= 8)
                        {
                            // Chưa chạy → Cyan Blue
                            step.StartAt = null;
                            step.EndAt = null;
                            step.TotalRunTime = null;
                        }
                        else
                        {
                            // Disabled → Black
                            step.Enanble = false;
                            step.Speed_Hz = 0;
                            step.SoLuongXung = 0;
                        }

                        steps.Add(step);
                    }

                    var revoRealtime = new RevoRealtimeModel()
                    {
                        RevoId = config.Id ?? 0,
                        RevoName = config.Name,
                        Path = config.Path,
                        PlcConnected = true,
                        Work = "WORK",
                        Part = "AU228-IR-F",
                        Rev = "A",
                        ColorCode = "",
                        Mandrel = "M541",
                        MandrelStart = "2.5",
                        Steps = steps
                    };

                    // Tìm hoặc tạo FT08 record
                    var existingFt08 = await _context.FT08_RevoRealtimes
                        .FirstOrDefaultAsync(x => x.C000_RevoId == config.Id);

                    if (existingFt08 != null)
                    {
                        existingFt08.C001_Data = JsonConvert.SerializeObject(revoRealtime);
                        updatedRealtimes.Add(existingFt08);
                    }
                    else
                    {
                        var newFt08 = new FT08_RevoRealtime()
                        {
                            Id = Guid.NewGuid(),
                            C000_RevoId = config.Id,
                            C001_Data = JsonConvert.SerializeObject(revoRealtime)
                        };
                        await _context.FT08_RevoRealtimes.AddAsync(newFt08);
                        updatedRealtimes.Add(newFt08);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    Message = "Mock data generated successfully",
                    UpdatedCount = updatedRealtimes.Count,
                    StepsInfo = "Steps 1-3: Completed (light blue), Steps 4-5: Running (white), Steps 6-10: Pending (gray), Steps 11-12: Disabled (black), Steps 13-20: No time (gray)"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test disconnect - Set ConnectionStatus = 0 cho một hoặc tất cả REVO
        /// </summary>
        [HttpPost("TestDisconnect")]
        public async Task<IActionResult> TestDisconnect([FromQuery] int? revoId = null)
        {
            try
            {
                var query = _context.FT08_RevoRealtimes.AsQueryable();
                
                if (revoId.HasValue)
                {
                    query = query.Where(x => x.C000_RevoId == revoId);
                }

                var ft08List = await query.ToListAsync();
                if (ft08List.Count == 0)
                {
                    return NotFound("No REVO data found");
                }

                var updatedCount = 0;
                foreach (var ft08 in ft08List)
                {
                    if (!string.IsNullOrEmpty(ft08.C001_Data))
                    {
                        var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                        if (revoRealtime != null)
                        {
                            revoRealtime.PlcConnected = false; // Disconnected
                            ft08.C001_Data = JsonConvert.SerializeObject(revoRealtime);
                            updatedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    Message = revoId.HasValue 
                        ? $"REVO {revoId} disconnected successfully" 
                        : "All REVOs disconnected successfully",
                    UpdatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test connect - Set ConnectionStatus = 1 cho một hoặc tất cả REVO
        /// </summary>
        [HttpPost("TestConnect")]
        public async Task<IActionResult> TestConnect([FromQuery] int? revoId = null)
        {
            try
            {
                var query = _context.FT08_RevoRealtimes.AsQueryable();
                
                if (revoId.HasValue)
                {
                    query = query.Where(x => x.C000_RevoId == revoId);
                }

                var ft08List = await query.ToListAsync();
                if (ft08List.Count == 0)
                {
                    return NotFound("No REVO data found");
                }

                var updatedCount = 0;
                foreach (var ft08 in ft08List)
                {
                    if (!string.IsNullOrEmpty(ft08.C001_Data))
                    {
                        var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                        if (revoRealtime != null)
                        {
                            revoRealtime.PlcConnected = true; // Connected
                            ft08.C001_Data = JsonConvert.SerializeObject(revoRealtime);
                            updatedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    Message = revoId.HasValue 
                        ? $"REVO {revoId} connected successfully" 
                        : "All REVOs connected successfully",
                    UpdatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Update step time để test realtime - tự động cập nhật step đang chạy
        /// </summary>
        [HttpPost("UpdateStepTime")]
        public async Task<IActionResult> UpdateStepTime([FromQuery] int revoId, [FromQuery] int stepIndex)
        {
            try
            {
                var ft08 = await _context.FT08_RevoRealtimes
                    .FirstOrDefaultAsync(x => x.C000_RevoId == revoId);

                if (ft08 == null || string.IsNullOrEmpty(ft08.C001_Data))
                {
                    return NotFound("REVO data not found");
                }

                var revoRealtime = JsonConvert.DeserializeObject<RevoRealtimeModel>(ft08.C001_Data);
                if (revoRealtime == null || revoRealtime.Steps == null)
                {
                    return BadRequest("Invalid REVO data");
                }

                var step = revoRealtime.Steps.FirstOrDefault(s => s.StepIndex == stepIndex);
                if (step == null)
                {
                    return NotFound($"Step {stepIndex} not found");
                }

                var now = DateTime.Now;
                
                // Set step đang chạy
                step.StartAt = now.AddMinutes(-2); // Bắt đầu 2 phút trước
                step.EndAt = now.AddMinutes(8); // Kết thúc sau 8 phút
                step.Enanble = true;
                step.Visible = true;

                // Set step trước đó đã hoàn thành
                var prevStep = revoRealtime.Steps.FirstOrDefault(s => s.StepIndex == stepIndex - 1);
                if (prevStep != null)
                {
                    prevStep.StartAt = now.AddMinutes(-15);
                    prevStep.EndAt = now.AddMinutes(-2);
                    prevStep.Enanble = true;
                    prevStep.Visible = true;
                }

                ft08.C001_Data = JsonConvert.SerializeObject(revoRealtime);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    Message = $"Step {stepIndex} updated to running state",
                    StartAt = step.StartAt,
                    EndAt = step.EndAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
