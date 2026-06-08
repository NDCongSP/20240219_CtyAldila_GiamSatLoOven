using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng lưu trữ Refresh Token cho từng user.
    /// Mỗi lần login sẽ tạo 1 bản ghi mới, khi refresh sẽ revoke cái cũ và tạo cái mới.
    /// </summary>
    [Table("FT17_RefreshTokens")]
    public class FT17_RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Chuỗi refresh token (GUID).
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Token { get; set; }

        /// <summary>
        /// UserId liên kết với bảng AspNetUsers.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        /// <summary>
        /// JTI (JWT ID) của access token tương ứng.
        /// </summary>
        [MaxLength(256)]
        public string? JwtId { get; set; }

        /// <summary>
        /// Đã sử dụng hay chưa (mỗi refresh token chỉ dùng 1 lần).
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Đã bị thu hồi (revoke) chưa.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Ngày tạo.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Ngày hết hạn.
        /// </summary>
        public DateTime ExpiryDate { get; set; }
    }
}
