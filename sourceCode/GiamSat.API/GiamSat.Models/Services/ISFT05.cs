using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT05.BasePath)]
    public interface ISFT05 : IRepository<Guid, FT05>
    {
    }
}
