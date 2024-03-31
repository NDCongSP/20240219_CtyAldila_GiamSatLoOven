using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class FilterModel
    {
        public bool GetAll { get; set; }
        public int OvenId { get; set; } = 1;
        public string OvenName { get; set; }
        public int ProfileId { get; set; } = 1;
        public string ProfileName { get; set; }
        public int StepId { get; set; } = 1;

        /// <summary>
        /// Lưu ý khi nhập ngày tháng để test trên postname thì phải thêm chữ T giữa ngày tháng và thời gian.
        /// "FromDate": "2024-03-20T01:23:42",
        /// </summary>
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
    }
}
