using RestEase;
using System;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT14.BasePath)]
    public interface ISFT14 : IRepository<Guid, FT14_TipOdFreq>
    {
    }
}
