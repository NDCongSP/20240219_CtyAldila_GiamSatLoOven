using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT05.BasePath)]
    public interface ISFT05 : IRepository<Guid, FT05>
    {
        [Post(ApiRoutes.FT05.GetFilter)]
        Task<Result<List< FT05>>> GetFilter([Body] FilterModel model);
    }
}
