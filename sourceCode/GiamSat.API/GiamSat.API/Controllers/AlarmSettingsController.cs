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
    public class AlarmSettingsController : BaseController<Guid, AlarmSettingsModel>, ISAlarmSettings
    {
        readonly SCommon _sCommon;
        public AlarmSettingsController(SCommon sCommon) : base(sCommon.SAlarmSettings)
        {
            _sCommon = sCommon;
        }
    }
}
