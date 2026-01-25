using RestEase;
using System;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT07.BasePath)]
    public interface ISFT07 : IRepository<Guid, FT07_RevoConfig>
    {
    }
}
