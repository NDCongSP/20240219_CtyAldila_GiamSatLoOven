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
    public class FT05Controller : BaseController<Guid, FT05>, ISFT05
    {
        readonly SCommon _sCommon;

        public FT05Controller(SCommon sCommon) : base(sCommon.SFT05)
        {
            _sCommon = sCommon;
        }

        [HttpPost(ApiRoutes.FT05.GetFilter)]
        public Task<Result<List<FT05>>> GetFilter([Body] FilterModel model)
        {
            throw new NotImplementedException();
        }
    }
}
