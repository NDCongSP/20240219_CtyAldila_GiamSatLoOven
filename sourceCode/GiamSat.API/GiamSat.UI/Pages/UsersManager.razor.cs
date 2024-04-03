using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using GiamSat.Models;
using Radzen;
using Radzen.Blazor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GiamSat.UI.Pages
{
    public partial class UsersManager
    {
        List<UserModel> _users = new List<UserModel>();
        UserModel _userModel = new UserModel();

        APIClient.RegisterModel _registerModel = new APIClient.RegisterModel();
        string _roleSelect;
        List<string> _role = new List<string>() { "User", "Operator" };

        RadzenDataGrid<UserModel> _profileGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 5, 10, 20, 30, 100, 200 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            RefreshData();

            //var res = await _authSerivce.GetAllUsers();

            //foreach (var item in res)
            //{
            //    _users.Add(new UserModel()
            //    {
            //        Id = item.Id,
            //        UserName = item.UserName,
            //        Email = item.Email,
            //    });
            //}

            //await InvokeAsync(async () =>
            //{
            //    await _profileGrid.RefreshDataAsync();
            //});
            //// StateHasChanged();
        }

        async Task DeleteItem(string id, string userName)
        {
            try
            {
                var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn xóa user: {userName}", "Xóa tài khoản", new ConfirmOptions()
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true,
                });

                if (confirm == null || confirm == false) return;

                var res = await _authSerivce.DeleteUser(new APIClient.UserModel()
                {
                    UserName = userName,
                    Id = id,
                });

                if (res.Status == "Success")
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = res.Message,
                        Duration = 4000
                    });

                    _registerModel = null;
                    _registerModel = new APIClient.RegisterModel();

                    RefreshData();
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = res.Message,
                        Duration = 4000
                    });
                }
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

        async Task EditItem(string id, string userName)
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

        async Task OnClickSaveAsync()
        {
            var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn tạo user: {_userModel.UserName}", "Tạo user", new ConfirmOptions()
            {
                OkButtonText = "Yes",
                CancelButtonText = "No",
                AutoFocusFirstElement = true,
            });

            if (confirm == null || confirm == false) return;

            _registerModel.Username = _userModel.UserName;
            _registerModel.Password = "123@123";
            _registerModel.Email = _userModel.Email;

            var res = await _authSerivce.RegisterUser(_registerModel);

            if (res.Status == "Success")
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = res.Message,
                    Duration = 4000
                });

                _registerModel = null;
                _registerModel = new APIClient.RegisterModel();

                RefreshData();
            }
            else
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = res.Message,
                    Duration = 4000
                });
            }
        }

        async void RefreshData()
        {
            try
            {
                var res = await _authSerivce.GetAllUsers();

                _users = null;
                _users = new List<UserModel>();

                foreach (var item in res)
                {
                    _users.Add(new Models.UserModel()
                    {
                        Id = item.Id,
                        UserName = item.UserName,
                        Email = item.Email,
                    });
                }

                await _profileGrid.RefreshDataAsync();
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
