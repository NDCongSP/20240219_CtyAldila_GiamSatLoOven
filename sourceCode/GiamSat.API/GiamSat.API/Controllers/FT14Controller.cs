using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT14Controller : BaseController<Guid, FT14_TipOdFreq>, ISFT14
    {
        readonly SCommon _sCommon;

        public FT14Controller(SCommon sCommon = null) : base(sCommon.SFT14)
        {
            _sCommon = sCommon;
        }
    }
}
