using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT101.BasePath)]
    public interface ISFT101 : IRepository<Guid, FT101>
    {
    }
}
