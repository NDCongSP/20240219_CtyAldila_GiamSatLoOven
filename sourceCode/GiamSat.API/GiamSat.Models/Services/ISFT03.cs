using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT03.BasePath)]
    public interface ISFT03 : IRepository<Guid, FT03>
    {
        [Post(ApiRoutes.FT03.GetFilter)]
        Task<Result<List<FT03>>> GetFilter([Body] FilterModel model);
    }
}
