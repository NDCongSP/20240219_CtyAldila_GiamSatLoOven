using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT01.BasePath)]
    public interface ISFT01:IRepository<Guid,FT01>
    {
    }
}
