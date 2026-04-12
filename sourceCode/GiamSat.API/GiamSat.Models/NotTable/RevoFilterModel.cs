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

        /// <summary>Phạm vi shaft báo cáo: "total" (tất cả) hoặc "finished" (chỉ shaft đã hoàn thành — mọi dòng TotalTime lớn hơn 0).</summary>
        public string? ShaftScope { get; set; } = "total";
    }
}
