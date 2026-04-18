using GiamSat.APIClient;
using GiamSat.Models;

namespace GiamSat.UI
{
    public static class GlobalVariable
    {
        //public static int RefreshInterval { get; set; } = 2000;
        //public static int ChartRefreshInterval { get; set; } = 1000;

        //public static int ChartPointNum { get; set;} = 10;

        //public static bool Smooth = false;
        //public static bool ShowDataLabels = false;
        //public static bool ShowMarkers = true;

        public static ConfigModel ConfigSystem { get; set; }
        public static RevoConfigModel RevoConfig { get; set; } = new RevoConfigModel();

        /// <summary>
        /// Timer refresh trang REVO (RevoHome, DialogRevoDetail). Đơn vị ms. Đọc từ appsettings.json.
        /// </summary>
        public static int RevoRefreshInterval { get; set; } = 10000;

        /// <summary>
        /// Timer refresh biểu đồ realtime (RealtimeTrend). Đơn vị ms. Đọc từ appsettings.json.
        /// </summary>
        public static int RealtimeTrendInterval { get; set; } = 1000;

        public static Guid UserId { get; set; }
        public static string UserName { get; set; } = "user";
        public static List<BreadCrumbModel> BreadCrumbData { get; set; }=new List<BreadCrumbModel>();
    }
}
