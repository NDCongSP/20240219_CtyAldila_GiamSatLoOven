namespace GiamSat.Models
{
    public enum EnumSandingLogType
    {
        /// <summary>
        /// Log tất cả cây shaft của part.
        /// </summary>
        Log_All = 1,

        /// <summary>
        /// mỗi part chỉ log 5 cây.
        /// </summary>
        Pilot_5 =2,

        /// <summary>
        /// Không lưu log nào cả, tức là máy mài sẽ hoạt động ở chế độ demo, chỉ để test mà không cần lưu lại dữ liệu log vào database.
        /// </summary>
        No_Log = 3
    }
}
