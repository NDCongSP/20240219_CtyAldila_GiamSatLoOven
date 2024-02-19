using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng này dùng để lưu các thông tin tổng của 1 lần nuôi. theo từng chuồng.
    /// Mỗi khi kết thức 1 chu trình nuôi, nhấn reset là sẽ insert 1 dòng mới vào.
    /// </summary>
    public class FT100
    {
        [Key]
        public Guid Id { get; set; }
        //public string Name { get; set; }
        ///// <summary>
        ///// Tổng số con của lần nuôi này.
        ///// </summary>
        //public int TongSoCon { get; set; }
        ///// <summary>
        /////List Model chứa các thông tin các chuồng đang nuôi cho chu trình này.
        /////Đây là list<model> ChuongNuoi
        ///// </summary>
        //public string Details { get; set; }
        ///// <summary>
        ///// Quản lý về thức ăn cho chu trình.
        ///// </summary>

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifyDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public int? IsActived { get; set; } = 1; 
        public string? CreatedMachine { get; set; }

    }
}
