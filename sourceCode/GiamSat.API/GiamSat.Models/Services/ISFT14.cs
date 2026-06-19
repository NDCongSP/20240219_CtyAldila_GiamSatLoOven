using RestEase;
using System;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT14.BasePath)]
    public interface ISFT14 : IRepository<Guid, FT14_TipOdFreq>
    {
        /// <summary>
        /// Xóa cứng một bản ghi FT14 khỏi database theo Id (không phải soft-delete Actived=0).
        /// </summary>
        [Delete(ApiRoutes.Delete)]
        Task<Result<bool>> Delete([Path] Guid id);
    }
}
