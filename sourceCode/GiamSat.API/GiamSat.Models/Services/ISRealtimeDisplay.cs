using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.RealtimeDisplay.BasePath)]
    public interface ISRealtimeDisplay:IRepository<Guid,RealtimeDisplayModel>
    {
        [Get(ApiRoutes.RealtimeDisplay.GetTop1)]
        Task<Result<RealtimeDisplayModel>> GetTop1(); 
    }
}
