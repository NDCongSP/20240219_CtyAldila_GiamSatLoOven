using System;

namespace GiamSat.Models
{
    public class RevoFilterModel
    {
        public bool GetAll { get; set; }
        public int? RevoId { get; set; }
        public string? RevoName { get; set; }

        /// <summary>
        /// Lưu ý khi nhập ngày tháng để test trên postman thì phải thêm chữ T giữa ngày tháng và thời gian.
        /// "FromDate": "2024-03-20T01:23:42",
        /// </summary>
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
    }
}
