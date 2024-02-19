using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.DisplayRealtime.BasePath)]
    public interface ISDisplayRealtime : IRepository<Guid, DisplayRealTimeModel>, IRepositoryService
    {
        [Post(ApiRoutes.DisplayRealtime.enUpdateTChuong)]
        Task<Result<DisplayRealTimeModel>> UpdateTenChuong(DisplayRealTimeModel model);
    }
}
