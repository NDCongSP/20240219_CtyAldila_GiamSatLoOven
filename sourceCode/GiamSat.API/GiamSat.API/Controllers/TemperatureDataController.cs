using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureDataController : ControllerBase
    {
        private readonly ISFT11 _service;

        public TemperatureDataController(ISFT11 service)
        {
            _service = service;
        }

        [HttpGet("GetRealtime")]
        public async Task<ActionResult<List<TemperatureRealtimeModel>>> GetRealtime()
        {
            var result = await _service.GetRealtime();
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(result.Data);
        }

        [HttpGet("GetAlarmLogs")]
        public async Task<ActionResult<List<FT13_TemperatureAlarmLog>>> GetAlarmLogs([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = await _service.GetAlarmLogs(fromDate, toDate);
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(result.Data);
        }

        [HttpGet("GetDataLogs")]
        public async Task<ActionResult<List<FT12_TemperatureDatalog>>> GetDataLogs([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = await _service.GetDataLogs(fromDate, toDate);
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(result.Data);
        }

        [HttpPost("SyncRealtime")]
        public async Task<IActionResult> SyncRealtime([FromBody] List<TemperatureRealtimeModel> reqData)
        {
            var result = await _service.SyncRealtime(reqData);
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(new { Message = "Sync Realtime Successfully", Timestamp = DateTime.Now });
        }

        [HttpPost("SyncDatalog")]
        public async Task<IActionResult> SyncDatalog([FromBody] List<TemperatureRealtimeModel> reqData)
        {
            var result = await _service.SyncDatalog(reqData);
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(new { Message = "Sync Datalog Successfully" });
        }
    }
}
