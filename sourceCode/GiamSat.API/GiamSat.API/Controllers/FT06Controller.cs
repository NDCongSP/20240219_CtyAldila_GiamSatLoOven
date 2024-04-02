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
    public class FT06Controller : BaseController<Guid, FT06>, ISFT06
    {
        readonly SCommon _sCommon;
        public FT06Controller( SCommon sCommon) : base(sCommon.SFT06)
        {
            _sCommon = sCommon;
        }
    }
}
