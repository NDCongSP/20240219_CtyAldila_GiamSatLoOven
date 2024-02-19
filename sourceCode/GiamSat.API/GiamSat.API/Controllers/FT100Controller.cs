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
    public class FT100Controller : BaseController<Guid, FT100>,ISFT100
    {
        readonly SCommon _sCommon;

        public FT100Controller(SCommon sCommon) : base(sCommon.SFT100)
        {
            _sCommon = sCommon;
        }
    }
}
