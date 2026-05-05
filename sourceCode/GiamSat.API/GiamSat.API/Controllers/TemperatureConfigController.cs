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
    public class TemperatureConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TemperatureConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the first active FT10_TemperatureConfig data and deserialize it into a list
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TemperatureConfigsModel>>> GetConfigs()
        {
            try
            {
                var ft10 = await _context.FT10_TemperatureConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft10 == null || string.IsNullOrEmpty(ft10.C000))
                {
                    return Ok(new List<TemperatureConfigsModel>()); // Return empty list if no config exists
                }

                var configs = JsonConvert.DeserializeObject<List<TemperatureConfigsModel>>(ft10.C000);
                return Ok(configs ?? new List<TemperatureConfigsModel>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        /// <summary>
        /// Save a list of TemperatureConfigsModel back to the FT10_TemperatureConfig C000 field
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TemperatureConfigsModel>> SaveConfigs([FromBody] List<TemperatureConfigsModel> configs)
        {
            try
            {
                var ft10 = await _context.FT10_TemperatureConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                var jsonString = JsonConvert.SerializeObject(configs);

                if (ft10 == null)
                {
                    // Create new row
                    ft10 = new FT10_TemperatureConfig
                    {
                        Id = Guid.NewGuid(),
                        C000 = jsonString,
                        Actived = true,
                        IsConfigChanged = true,
                        CreatedAt = DateTime.Now
                    };
                    await _context.FT10_TemperatureConfigs.AddAsync(ft10);
                }
                else
                {
                    // Update existing row
                    ft10.C000 = jsonString;
                    ft10.IsConfigChanged = true;
                    _context.FT10_TemperatureConfigs.Update(ft10);
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Configuration saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
