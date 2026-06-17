using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Service đồng bộ thông số Part từ external DB ALD_MFG (Part / PartZM / ZMmeasType / PartNewSetting)
    /// vào bảng master Oven.FT14.
    /// </summary>
    public interface ISFT14_Sync
    {
        /// <summary>
        /// Lấy danh sách Part nguồn (PartId + PartName) từ external DB để chia batch đồng bộ.
        /// </summary>
        Task<Result<List<PartSyncSourceDto>>> GetSyncSourcesAsync();

        /// <summary>
        /// Đồng bộ một batch Part (theo PartId) vào FT14: insert nếu chưa có, update nếu data thay đổi,
        /// bỏ qua nếu không đổi. Giữ nguyên A/B/C/D/Formula/Z_Stiffness (do người dùng tự tính).
        /// </summary>
        Task<Result<FT14SyncResultDto>> SyncPartsAsync(List<int> partIds);
    }
}
