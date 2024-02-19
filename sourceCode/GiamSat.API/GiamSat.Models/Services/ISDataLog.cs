using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.DataLog.BasePath)]
    public interface ISDataLog : IRepository<Guid, DataLogModel>
    {
        [Get(ApiRoutes.DataLog.GetFromToByName)]
        Task<Result<List<DataLogModel>>> GetFromToByName([Path] string from,string to, string tenChuong);
    }
}
