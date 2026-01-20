using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT09.BasePath)]
    public interface ISFT09 : IRepository<Guid, FT09_RevoDatalog>
    {
        Task<Result<List<FT09_RevoDatalog>>> GetFilter([Body] RevoFilterModel model);
    }
}
