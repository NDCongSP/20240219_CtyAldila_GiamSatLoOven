using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT08Controller : BaseController<Guid, FT08_RevoRealtime>, ISFT08
    {
        readonly SCommon _sCommon;

        public FT08Controller(SCommon sCommon = null) : base(sCommon.SFT08)
        {
            _sCommon = sCommon;
        }
    }
}
