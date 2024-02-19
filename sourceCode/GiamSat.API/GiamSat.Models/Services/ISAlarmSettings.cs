using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.AlarmSettings.BasePath)]
    public interface ISAlarmSettings : IRepository<Guid, AlarmSettingsModel>
    {
    }
}
