using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT16.BasePath)]
    public interface ISFT16:IRepository<Guid, FT016_SandingLogData>
    {
    }
}
