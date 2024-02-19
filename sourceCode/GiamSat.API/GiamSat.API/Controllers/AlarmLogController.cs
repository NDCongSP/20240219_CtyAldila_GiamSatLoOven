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
    public class AlarmLogController : BaseController<Guid, AlarmLogModel>, ISAlarmLog
    {
        readonly SCommon _sCommon;
        public AlarmLogController(SCommon sCommon = null) : base(sCommon.SAlarmLog)
        {
            _sCommon = sCommon;
        }
    }
}
