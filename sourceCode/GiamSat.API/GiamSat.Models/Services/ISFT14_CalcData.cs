using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Service lấy dữ liệu đo tần số từ external DB để tính A,B,C,D.
    /// </summary>
    public interface ISFT14_CalcData
    {
        /// <summary>
        /// Query Fre1 (Station="Auto Fre No.1") và Fre2 (Station="Auto Fre No.2")
        /// từ external DB theo Part + WorkOrder, ghép với RPM range.
        /// </summary>
        Task<Result<List<AutoSandingTestRow>>> GetCalcDataAsync(
            string part,
            string work,
            double offsetFre1,
            double offsetFre2,
            double motorFrom,
            double motorTo,
            double motorStep);
    }
}
