using GiamSat.APIClient;
using Radzen.Blazor;
using System.Numerics;

namespace GiamSat.UI.Pages
{
    public partial class Report
    {
        private List<APIClient.FT03> _dataReport = new List<APIClient.FT03>();

        RadzenDataGrid<APIClient.FT03> _dataReportGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 10, 50, 100, 200 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        async void QueryData()
        {
            try
            {
                var res= await _ft03Client.GetAllAsync();

                if (!res.Succeeded)
                {
                    _notificationService.Notify(new Radzen.NotificationMessage()
                    {
                        Severity = Radzen.NotificationSeverity.Error,
                        Summary ="Error",
                        Detail = "Truy cập API lỗi.",
                        Duration = 4000
                    });
                    return;
                }

                _dataReport = res.Data.ToList();
                
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new Radzen.NotificationMessage()
                {
                    Severity = Radzen.NotificationSeverity.Error,
                    Summary = ex.Message,
                    Detail = ex.StackTrace,
                    Duration = 4000
                });
                return;
            }

        }
    }
}
