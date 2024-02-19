using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RealtimeDisplayController : BaseController<Guid, RealtimeDisplayModel>, ISRealtimeDisplay
    {
        readonly SCommon _sCommon;

        public RealtimeDisplayController(SCommon sCommon):base(sCommon.SRealtimeDisplay)
        {
            _sCommon = sCommon;
        }

        [HttpGet(ApiRoutes.RealtimeDisplay.GetTop1)]
        public Task<Result<RealtimeDisplayModel>> GetTop1()
        {
            return _sCommon.SRealtimeDisplay.GetTop1();
        }
    }
}
