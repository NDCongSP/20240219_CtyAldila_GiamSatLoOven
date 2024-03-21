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
    public class FT03Controller : BaseController<Guid, FT03>, ISFT03
    {
        readonly SCommon _sCommon;

        public FT03Controller(SCommon sCommon = null) : base(sCommon.SFT03)
        {
            this._sCommon = sCommon;
        }
    }
}
