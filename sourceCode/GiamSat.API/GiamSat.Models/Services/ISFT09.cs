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

        [Post("GetReportStepView")]
        Task<PagedResult<RevoReportStepVm>> GetReportStepView([Body] RevoFilterModel model);

        [Post("GetReportShaftView")]
        Task<PagedResult<RevoReportShaftVm>> GetReportShaftView([Body] RevoFilterModel model);

        [Post("GetReportHourView")]
        Task<PagedResult<RevoReportHourVm>> GetReportHourView([Body] RevoFilterModel model);

        [Get("GetTotalShaft")]
        Task<Result<List<RevoGetTotalShaftCountDto>>> GetTotalShaft([Query] int? revoId = null);
    }
}
