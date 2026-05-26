using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureConfigController : ControllerBase
    {
        private readonly ISFT10 _service;

        public TemperatureConfigController(ISFT10 service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy cấu hình nhiệt độ (toàn bộ location config từ FT10.C000).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<TemperatureConfigsModel>> GetConfigs()
        {
            var result = await _service.GetConfigs();
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(result.Data);
        }

        /// <summary>
        /// Lưu cấu hình nhiệt độ vào FT10.C000.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveConfigs([FromBody] TemperatureConfigsModel config)
        {
            var result = await _service.SaveConfigs(config);
            if (!result.Succeeded) return StatusCode(500, new { Error = result.Messages });
            return Ok(new { Message = "Configuration saved successfully." });
        }
    }
}
