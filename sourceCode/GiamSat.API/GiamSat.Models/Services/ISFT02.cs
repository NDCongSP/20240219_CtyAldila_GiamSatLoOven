using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT02.BasePath)]
    public interface ISFT02 : IRepository<Guid, FT02>
    {
    }
}
