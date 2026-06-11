using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT16Controller : BaseController<Guid, FT16_SandingLogData>, ISFT16
    {
        readonly SCommon _sCommon;

        public FT16Controller(SCommon sCommon = null) : base(sCommon.SFT16)
        {
            _sCommon = sCommon;
        }

        [HttpGet(ApiRoutes.FT16.GetReport)]
        public async Task<Result<System.Collections.Generic.List<FT16_SandingLogData>>> GetReport(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] EnumSandingMode? mode)
        {
            return await _sCommon.SFT16.GetReport(from, to, mode);
        }
    }
}
