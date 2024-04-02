using DocumentFormat.OpenXml.Spreadsheet;
using GiamSat.Models;
using Radzen;
using Radzen.Blazor;

namespace GiamSat.UI.Pages
{
    public partial class UsersManager
    {
        List<UserModel> _users = new List<UserModel>();
        UserModel _userModel = new UserModel();

        //APIClient.RegisterModel _registerModel = new APIClient.RegisterModel();
        string _roleSelect;

        RadzenDataGrid<UserModel> _profileGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 5, 10, 20, 30,100,200 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var res = await _authSerivce.GetAllUsers();

            foreach (var item in res)
            {
                _users.Add(new UserModel()
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Email = item.Email,
                });
            }
        }

        async Task DeleteItem(string id,string userName)
        {
            try
            {
                var res = await _authSerivce.DeleteUser(new APIClient.UserModel()
                {
                    UserName= userName,
                    Id=id,
                });
                
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

                StateHasChanged();
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
        }
    }
}
