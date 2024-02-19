using System;

namespace GiamSat.Models
{
    public class NangSuatModel
    {
        public Guid ChuongId { get; set; }

        /// <summary>
        /// Số lượng con giống thả ban đầu khi bắt đầu nuôi.
        /// </summary>
        public int SoLuongBanDau { get; set; }
        /// <summary>
        /// số lượng con chế
        /// </summary>
        public int SoLuongChet { get; set; }
        /// <summary>
        /// số lượng con còn lại.
        /// </summary>
        public int SoLuongConLai { set { } get { return SoLuongBanDau - SoLuongChet; } }
        /// <summary>
        /// Số lượng con hao hụt.
        /// </summary>
        public int SoLuongHaoHut { set { } get { return SoLuongChet; } }
        /// <summary>
        /// số lượng con bị bệnh.
        /// </summary>
        public int SoLuongBiBenh { get; set; }
        /// <summary>
        /// số lượng con đang điều trị bệnh.
        /// </summary>
        public int SoLuongDangDieuTri { get; set; }

        public string TenThucAn { get; set; }
        /// <summary>
        /// Lượng thức ăn 1 con ăn 1 ngày.
        /// </summary>
        public double KhoiLuongTren1Con { get; set; }
        /// <summary>
        /// Tổng khối lượng thức ăn cài đặt khi bắt đầu nuôi.
        /// </summary>
        public double TongKhoiLuongThucAn { get; set; }
        /// <summary>
        /// Tổng khối lượng đo từ cân.
        /// </summary>
        public double TongKhoiLuongThucTe { get; set; }
        /// <summary>
        /// Khối lượng thức ăn đã dùng.
        /// </summary>
        public double TongKhoiLuongThucAnDaSuDung
        {
            set { }
            get { return TongKhoiLuongThucAn - TongKhoiLuongThucTe; }
        }
        /// <summary>
        /// Khối lượng chốt vào 0h sáng.
        /// </summary>
        public double ChotKhoiLuongThucAnDauNgay { get; set; }
        /// <summary>
        /// Khối lượng thức ăn chốt vào 23:59:59.
        /// </summary>
        public double ChotKhoiLuongThucAnCuoiNgay { get; set; }
        /// <summary>
        /// Khối lượng thức ăn dùng trong 1 ngày.
        /// </summary>
        public double KhoiLuongThucAnDungTrongNgay
        {
            set { }
            get { return Math.Round(ChotKhoiLuongThucAnCuoiNgay - ChotKhoiLuongThucAnDauNgay, 2); }
        }
        /// <summary>
        /// Tổng khối lượng thức ăn cần dùng trong 1 ngày theo đầu con còn lại trong chuồng.
        /// </summary>
        public double KhoiLuongThucAnTheoDauCon { set { } get { return Math.Round(SoLuongConLai * KhoiLuongTren1Con, 2); } }
    }
}