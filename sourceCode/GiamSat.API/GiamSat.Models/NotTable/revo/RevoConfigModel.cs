using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class RevoConfigs : List<RevoConfigModel>
    {

    }

    public class RevoConfigModel
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string Path { get; set; }

        public string? ConstringAccessDb { get; set; }

        /// <summary>
        /// độ phân giải của driver trên máy REVO (pulse/rev).
        /// </summary>
        public int Pulse_Rev { get; set; } = 3200;

        public int IntervalResetShaft { get; set; } = 3000;

        /// <summary>
        /// Tùy chọn để lưu thời gian của 1 cây shaft, hay là lưu chi tiết từng bước.
        /// </summary>
        public EnumSaveMode SaveMode { get; set; } = EnumSaveMode.Save;

        /// <summary>
        /// Offset số xung/1 vòng, dùng để tính toán cho phần tốc độ truyền xuống PLC.
        /// </summary>
        public int Pulse_rev_Offset { get; set; } = 0;

        /// <summary>
        /// Quy định loại máy để lấy thông tin cấu hình phù hợp, và lưu trữ data theo loại máy.
        /// </summary>
        public EnumMachineType MachineType { get; set; } = EnumMachineType.REVO;
    }
}
