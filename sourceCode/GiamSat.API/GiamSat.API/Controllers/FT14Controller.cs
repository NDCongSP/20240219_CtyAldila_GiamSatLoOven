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
    public class FT14Controller : BaseController<Guid, FT14_TipOdFreq>, ISFT14
    {
        readonly SCommon _sCommon;
        readonly ISFT14_CalcData _calcDataService;

        public FT14Controller(SCommon sCommon = null, ISFT14_CalcData calcDataService = null) : base(sCommon.SFT14)
        {
            _sCommon = sCommon;
            _calcDataService = calcDataService;
        }

        /// <summary>
        /// Lấy dữ liệu Fre1/Fre2 từ external DB để tính A,B,C,D.
        /// </summary>
        [HttpGet("calcdata")]
        public async Task<Result<List<AutoSandingTestRow>>> GetCalcData(
            [FromQuery] string part,
            [FromQuery] string work,
            [FromQuery] double offsetFre1 = 0,
            [FromQuery] double offsetFre2 = 0,
            [FromQuery] double motorFrom  = 100,
            [FromQuery] double motorTo    = 500,
            [FromQuery] double motorStep  = 100)
        {
            return await _calcDataService.GetCalcDataAsync(part, work, offsetFre1, offsetFre2, motorFrom, motorTo, motorStep);
        }
    }
}
