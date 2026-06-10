using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT16.BasePath)]
    public interface ISFT16 : IRepository<Guid, FT16_SandingLogData>
    {
        [Get(ApiRoutes.FT16.GetReport)]
        Task<Result<List<FT16_SandingLogData>>> GetReport(
            [Query] DateTime? from,
            [Query] DateTime? to,
            [Query] EnumSandingMode? mode);
    }
}
