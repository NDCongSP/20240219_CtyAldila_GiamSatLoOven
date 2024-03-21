using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT02Controller : BaseController<Guid, FT02>, ISFT02
    {
        readonly SCommon _sCommon;

        public FT02Controller(SCommon sCommon = null) : base(sCommon.SFT02)
        {
            _sCommon = sCommon;
        }
    }
}
