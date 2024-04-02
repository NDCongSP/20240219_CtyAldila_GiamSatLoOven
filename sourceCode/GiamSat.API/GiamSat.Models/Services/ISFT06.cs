using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT06.BasePath)]
    public interface ISFT06:IRepository<Guid,FT06>
    {
    }
}
