namespace GiamSat.Models
{
    public class TemperatureRealtimeModel
    {
        /// <summary>
        /// Location ID, để phân biệt các vị trí đo nhiệt độ khác nhau trong nhà máy, ví dụ: 1, 2, 3,... tương ứng với các vị trí như: lò nung, khu vực sản xuất, kho chứa,...
        /// TemperatureConfigsModel.Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Location name, tên của vị trí đo nhiệt độ, để hiển thị trên UI và dễ dàng nhận biết, ví dụ: "Lò nung 1", "Khu vực sản xuất A", "Kho chứa B",...
        /// TemperatureConfigsModel.Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// địa chỉ easy driver server.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Báo trạng thái kết nối đến đồng hồ đo nhiệt độ, để hiển thị trạng thái kết nối trên UI, và có thể đưa ra cảnh báo nếu mất kết nối.
        /// </summary>
        public bool Status { get; set; } = false;

        /// <summary>
        /// Báo cáo trạng thái kết nối đến Easy Driver server để lấy dữ liệu nhiệt độ.
        /// </summary>
        public bool ConnectionStatus { get; set; } = false;

        /// <summary>
        /// Giá trị nhiệt độ đo được từ cảm biến, đã được hiệu chỉnh với offset nếu có.
        /// </summary>
        public double PV { get; set; } = 0;

        /// <summary>
        /// Tín hiệu cảnh báo khi nhiệt độ vượt quá ngưỡng cao hoặc thấp, để hiển thị cảnh báo trên UI.
        /// </summary>
        public bool Alarm { get; set; } = false;

        /// <summary>
        /// Cờ chốt để tránh việc lặp lại cảnh báo liên tục khi nhiệt độ vượt ngưỡng, chỉ báo hiệu lần đầu tiên vượt ngưỡng để hiển thị cảnh báo, sau đó sẽ không hiển thị lại cho đến khi nhiệt độ trở về bình thường và vượt ngưỡng lần nữa.
        /// </summary>
        public bool AlarmFlag { get; set; } = false;

        /// <summary>
        /// Mô tả chi tiết về nguyên nhân cảnh báo, ví dụ: "Nhiệt độ vượt quá ngưỡng cao 45°C", hoặc "Nhiệt độ thấp hơn ngưỡng thấp -30°C", để người dùng hiểu rõ lý do cảnh báo và có biện pháp xử lý phù hợp.
        /// </summary>
        public string AlarmDescription { get; set; }

        /// <summary>
        /// chứa các thông số cài đặt liên quan đến việc đo nhiệt độ, như offset, ngưỡng cảnh báo, đường dẫn easy driver,... để có thể hiển thị và cảnh báo phù hợp.
        /// </summary>
        public TemperatureConfigsModel Config { get; set; }
    }
}
