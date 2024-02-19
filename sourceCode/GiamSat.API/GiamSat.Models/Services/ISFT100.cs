using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT100.BasePath)]
    public interface ISFT100:IRepository<Guid,FT100>
    {
    }
}
