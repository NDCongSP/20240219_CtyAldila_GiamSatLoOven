using RestEase;
using System;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT08.BasePath)]
    public interface ISFT08 : IRepository<Guid, FT08_RevoRealtime>
    {
    }
}
