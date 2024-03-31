using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost(ApiRoutes.FT04.GetFilter)]
        public Task<Result<List<FT04>>> GetFilter([Body] FilterModel model)
        {
            return _sCommon.SFT04.GetFilter(model);
        }
    }
}
