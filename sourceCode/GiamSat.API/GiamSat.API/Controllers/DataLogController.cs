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
    //[AllowAnonymous]//khong can authorize
    public class DataLogController : BaseController<Guid, DataLogModel>, ISDataLog
    {
        readonly SCommon _sCommon;
        public DataLogController(SCommon sCommon) : base(sCommon.SDataLog)
        {
            _sCommon = sCommon;
        }

        [HttpGet(ApiRoutes.DataLog.GetFromToByName)]
        public Task<Result<List<DataLogModel>>> GetFromToByName([Path] string from, string to, string tenChuong)
        {
            return  _sCommon.SDataLog.GetFromToByName(from, to, tenChuong);
        }
    }
}
