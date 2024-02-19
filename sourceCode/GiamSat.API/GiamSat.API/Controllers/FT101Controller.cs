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
    public class FT101Controller : BaseController<Guid, FT101>, ISFT101
    {
        readonly SCommon _sCommon;
        public FT101Controller(SCommon sCommon) : base(sCommon.SFT101)
        {
            _sCommon = sCommon;
        }
    }
}
