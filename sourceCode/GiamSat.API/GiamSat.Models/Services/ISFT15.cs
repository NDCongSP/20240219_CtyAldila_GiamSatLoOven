using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT15.BasePath)]
    public interface ISFT15:IRepository<Guid, FT015_SandingRealtime>
    {
    }
}
