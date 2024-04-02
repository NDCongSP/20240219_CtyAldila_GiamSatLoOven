using DocumentFormat.OpenXml.Spreadsheet;
using GiamSat.APIClient;
using Radzen;
using Radzen.Blazor;

namespace GiamSat.UI.Pages
{
    public partial class UsersManager
    {
        List<Models.UserModel> _users = new List<Models.UserModel>();
        Models.UserModel _userModel = new Models.UserModel();

        RegisterModel _registerModel = new RegisterModel();

        RadzenDataGrid<Models.UserModel> _profileGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 5, 10, 20, 30,100,200 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RefreshData();
        }

        async Task DeleteItem(string userName)
        {
            try
            {
              
                
                RefreshData();
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 4000
                });

                return;
            }
        }

        async Task EditItem(string id,string userName)
        {
            try
            {
                var confirm = await _dialogService.Confirm("Bạn chắc chắn muốn thêm profile?", "Tạo mới profile", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

               
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 4000
                });
                return;
            }
        }

        void OnClickSave()
        {

        }

        async void RefreshData()
        {
            try
            {
                var res = await _authSerivce.GetAllUsers();

                foreach (var item in res)
                {
                    _users.Add(new Models.UserModel()
                    {
                        Id=item.Id,
                        UserName=item.UserName,
                        Email=item.Email,
                    });
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 4000
                });
                return;
            }

            StateHasChanged();
        }
    }
}
