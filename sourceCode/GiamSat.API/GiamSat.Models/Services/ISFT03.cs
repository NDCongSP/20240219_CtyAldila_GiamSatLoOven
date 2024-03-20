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
    }
}
