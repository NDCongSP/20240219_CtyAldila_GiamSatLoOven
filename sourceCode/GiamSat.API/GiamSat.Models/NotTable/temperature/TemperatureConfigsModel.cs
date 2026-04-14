namespace GiamSat.Models
{
    public class TemperatureConfigsModel
    {
        /// <summary>
        /// Location ID, để phân biệt các vị trí đo nhiệt độ khác nhau trong nhà máy, ví dụ: 1, 2, 3,... tương ứng với các vị trí như: lò nung, khu vực sản xuất, kho chứa,...
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Location name, tên của vị trí đo nhiệt độ, để hiển thị trên UI và dễ dàng nhận biết, ví dụ: "Lò nung 1", "Khu vực sản xuất A", "Kho chứa B",...
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// đường dẫn đến Easy driver server của mỗi vị trí đo nhiệt độ, để lấy data về.
        /// ex: Local Station/Channel_Revo/Device1
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Giá trị offset để hiệu chỉnh lại nhiệt độ đo được, nếu có sai số.
        /// </summary>
        public double Offset { get; set; } = 0;

        /// <summary>
        /// Ngưỡng nhiệt độ cao, nếu nhiệt độ đo được vượt quá ngưỡng này, sẽ có cảnh báo.
        /// </summary>
        public double HightLevel { get; set; } = 45;

        /// <summary>
        /// Ngưỡng nhiệt độ thấp, nếu nhiệt độ đo được thấp hơn ngưỡng này, sẽ có cảnh báo.
        /// </summary>
        public double LowLevel { get; set; } = -30;

        /// <summary>
        /// Thời gian nhấp nháy cảnh báo trên UI khi nhiệt độ vượt quá ngưỡng cao hoặc thấp (ms), để thu hút sự chú ý của người dùng.
        /// </summary>
        public double TimeBlinkAlarm { get; set; } = 1000;
    }
}
