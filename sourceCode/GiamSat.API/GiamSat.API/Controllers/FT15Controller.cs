using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT15Controller : BaseController<Guid, FT15_SandingRealtime>, ISFT15
    {
        readonly SCommon _sCommon;

        public FT15Controller(SCommon sCommon = null) : base(sCommon.SFT15)
        {
            _sCommon = sCommon;
        }
    }
}
