using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GiamSat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FT01Controller : BaseController<Guid,FT01>,ISFT01
    {
        readonly SCommon _sCommon;

        public FT01Controller(SCommon sCommon=null):base(sCommon.SFT01)
        {
            _sCommon = sCommon;
        }
    }
}
