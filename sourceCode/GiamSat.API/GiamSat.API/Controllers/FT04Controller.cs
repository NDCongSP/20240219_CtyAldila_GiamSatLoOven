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
    public class FT04Controller : BaseController<Guid,FT04>,ISFT04
    {
        readonly SCommon _sCommon;

        public FT04Controller(SCommon sCommon):base(sCommon.SFT04)
        {
            _sCommon = sCommon;
        }
    }
}
