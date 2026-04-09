using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class RevoRealtimeModel
    {
        public int RevoId { get; set; }
        public string RevoName { get; set; } = string.Empty;

        /// <summary>
        /// địa chỉ easy driver của máy REVO.
        /// </summary>
        public string? Path { get; set; } = string.Empty;

        /// <summary>
        /// trạng thái kết nối modbus đến máy.
        /// true-Kết nối; false - mất kết nối.
        /// </summary>
        public bool PlcConnected { get; set; }

        public string? Part { get; set; } = string.Empty;

        public string? Work { get; set; } = string.Empty;

        public string? Rev { get; set; } = string.Empty;

        public string? ColorCode { get; set; } = string.Empty;

        public string? Mandrel { get; set; } = string.Empty;

        public string? MandrelStart { get; set; } = string.Empty;

        public List<RevoStep> Steps { get; set; } = new List<RevoStep>();

        /// <summary>
        /// đánh dấu cây shaft mà dữ liệu này thuộc về.
        /// cá bước trong 1 cây shaft thì có cùng 1 ShaftNum.
        /// </summary>
        public Guid? ShaftNum { get; set; } = Guid.Empty;
    }

    public class RevoStep
    {
        public int StepIndex { get; set; }

        public string? StepName { get; set; } = string.Empty;

        /// <summary>
        /// REVO-STEP-1|0|0|H
        /// REVO-STEP-4|1080|720|N 4 là bước 4, 1080 là góc quay, 720 là tốc độ quay (anh cũng chưa rõ đơn vị anh đoán là RPM).
        /// </summary>
        public string? StepConfig { get; set; } = string.Empty;

        /// <summary>
        /// cho phép hiển thị hay không.
        /// </summary>
        public bool? Visible { get; set; } = false;

        /// <summary>
        /// Cho phép chay hay không.
        /// </summary>
        public bool? Enable { get; set; } = false;

        /// <summary>
        /// tốc độ, số vòng quay trên giây.
        /// </summary>
        public double? Speed_Hz { get; set; } = 0;

        /// <summary>
        /// =0 là quay mãi.
        /// Góc quay.số vòng quay.
        /// </summary>
        public double? SoLuongXung { get; set; } = 0;

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        /// <summary>
        /// Tổng thời gian chạy bước (giây).
        /// =StartAt - EndAt
        /// </summary>
        public double? TotalRunTime { get; set; } = 0;

        /// <summary>
        /// The hien buoc nao dang chon.
        /// </summary>
        public bool StepSelection { get; set; } = false;
    }
}
