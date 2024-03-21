using GiamSat.APIClient;
using GiamSat.Models;

namespace GiamSat.UI
{
    public static class GlobalVariable
    {
        public static int RefreshInterval { get; set; } = 2000;
        public static int ChartRefreshInterval { get; set; } = 1000;

        public static int ChartPointNum { get; set;} = 10;
    }
}
