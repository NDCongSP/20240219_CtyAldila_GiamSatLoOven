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

                foreach (var config in revoConfigs)
                {
                    var steps = new List<RevoStep>();
                    
                    // Tạo 20 steps với các trạng thái khác nhau
                    for (int j = 1; j <= 20; j++)
                    {
                        var step = new RevoStep()
                        {
                            StepIndex = j,
                            StepName = $"REVO-STEP-{j}",
                            StepConfig = $"REVO-STEP-{j}|0|0|H",
                            Visible = true,
                            Enanble = true,
                            Speed_Hz = 0,
                            SoLuongXung = 0,
                            StartAt = null,
                            EndAt = null
                        };

                        // Phân bổ các trạng thái step:
                        // Steps 1-3: Đã chạy xong (endAt < now) - màu xanh da trời
                        // Steps 4-5: Đang chạy (startAt <= now <= endAt) - màu trắng
                        // Steps 6-10: Chưa chạy (startAt > now) - màu xám
                        // Steps 11-12: Disabled (Enanble = false) - màu đen
                        // Steps 13-20: Chưa có thời gian - màu xám

                        if (j <= 3)
                        {
                            // Đã chạy xong
                            step.StartAt = now.AddMinutes(-(30 - j * 5)); // Cách nhau 5 phút
                            step.EndAt = now.AddMinutes(-(10 - j * 2)); // Kết thúc trước 2-6 phút
                        }
                        else if (j >= 4 && j <= 5)
                        {
                            // Đang chạy
                            step.StartAt = now.AddMinutes(-5); // Bắt đầu 5 phút trước
                            step.EndAt = now.AddMinutes(5); // Kết thúc sau 5 phút
                        }
                        else if (j >= 6 && j <= 10)
                        {
                            // Chưa chạy
                            step.StartAt = now.AddMinutes(10 + (j - 6) * 5); // Bắt đầu sau 10-30 phút
                            step.EndAt = now.AddMinutes(20 + (j - 6) * 5); // Kết thúc sau 20-40 phút
                        }
                        else if (j >= 11 && j <= 12)
                        {
                            // Disabled
                            step.Enanble = false;
                        }
                        // Steps 13-20: Không có StartAt/EndAt (màu xám mặc định)

                        steps.Add(step);
                    }

                    var revoRealtime = new RevoRealtimeModel()
                    {
                        RevoId = config.Id ?? 0,
                        RevoName = config.Name,
                        Path = config.Path,
                        ConnectionStatus = 1, // Connected
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
                            revoRealtime.ConnectionStatus = 0; // Disconnected
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
                            revoRealtime.ConnectionStatus = 1; // Connected
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
