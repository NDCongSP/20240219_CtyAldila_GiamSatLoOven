using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.ChuongInfo.BasePath)]
    public interface ISChuongInfo : IRepository<Guid, ChuongInfoModel>
    {
        [Get(ApiRoutes.ChuongInfo.GetByName)]
        Task<Result<ChuongInfoModel>> GetByName([Path]string id);
    }
}
