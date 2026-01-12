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
        public string RevoName { get; set; }

        public string? Path { get; set; }

        /// <summary>
        /// trạng thái kết nối modbus đến máy.
        /// 1-Kết nối; 0 - mất kết nối.
        /// </summary>
        public int ConnectionStatus { get; set; }

        public string? Work { get; set; }

        public string? Part { get; set; }

        public string? Rev { get; set; }

        public string? ColorCode { get; set; }

        public string? Mandrel { get; set; }

        public string? MandrelStart { get; set; }

        public List<RevoStep> Steps { get; set; }
    }

    public class RevoStep
    {
        public int StepIndex { get; set; }

        public string? StepName { get; set; }

        /// <summary>
        /// REVO-STEP-1|0|0|H
        /// REVO-STEP-4|1080|720|N 4 là bước 4, 1080 là góc quay, 720 là tốc độ quay (anh cũng chưa rõ đơn vị anh đoán là RPM).
        /// </summary>
        public string? StepConfig { get; set; }

        /// <summary>
        /// Cho phép chay hay không.
        /// </summary>
        public bool? Enanble { get; set; } = false;

        public double? Speed_Hz { get; set; }

        /// <summary>
        /// =0 là quay mãi,
        /// </summary>
        public double? SoLuongXung { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }
    }
}
