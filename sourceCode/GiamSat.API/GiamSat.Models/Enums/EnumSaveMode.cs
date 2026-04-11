using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public enum EnumSaveMode
    {
        /// <summary>
        /// chỉ lưu thời gian tổng của cây shaft.
        /// </summary>
        Save = 1,

        /// <summary>
        /// lưu chi tiết thời gian của từng bước (P1, P2, P3,...)
        /// </summary>
        SaveAll = 2
    }
}
