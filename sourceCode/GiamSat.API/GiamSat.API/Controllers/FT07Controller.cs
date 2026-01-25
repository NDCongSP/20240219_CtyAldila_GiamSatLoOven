using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT07Controller : BaseController<Guid, FT07_RevoConfig>, ISFT07
    {
        readonly SCommon _sCommon;

        public FT07Controller(SCommon sCommon = null) : base(sCommon.SFT07)
        {
            _sCommon = sCommon;
        }
    }
}
