using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

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
    }
}
