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
        /// Query Fre1/Fre2 từ external DB và StiffnessY từ FT16.
        /// Mỗi nguồn dữ liệu có WorkOrder riêng (workFre1, workFre2, workSpine).
        /// </summary>
        Task<Result<List<AutoSandingTestRow>>> GetCalcDataAsync(
            string part,
            string workFre1,
            string workFre2,
            string workSpine,
            double offsetFre1,
            double offsetFre2,
            double motorFrom,
            double motorTo,
            double motorStep);
    }
}
