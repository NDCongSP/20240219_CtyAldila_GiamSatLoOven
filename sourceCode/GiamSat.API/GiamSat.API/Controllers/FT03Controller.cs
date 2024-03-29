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
   //[Authorize]//được tạo bên BaseController
    [Route("api/[controller]")]
    [ApiController]
    public class FT03Controller : BaseController<Guid, FT03>, ISFT03
    {
        readonly SCommon _sCommon;

        public FT03Controller(SCommon sCommon = null) : base(sCommon.SFT03)
        {
            this._sCommon = sCommon;
        }

        [HttpPost(ApiRoutes.FT03.GetFilter)]
        public Task<Result<List<FT03>>> GetFilter([FromBody]FilterModel model)
        {
            return _sCommon.SFT03.GetFilter(model);
        }
    }
}
