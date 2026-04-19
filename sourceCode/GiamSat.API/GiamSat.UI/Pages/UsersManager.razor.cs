using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using GiamSat.Models;
using GiamSat.UI.Components;
using GiamSat.UI.Model;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GiamSat.UI.Pages
{
    public partial class UsersManager : IDisposable
    {
        private bool _disposed;
        List<APIClient.IdentityUserDto> _users = new List<APIClient.IdentityUserDto>();
        APIClient.IdentityUserDto _userModel = new APIClient.IdentityUserDto();

        APIClient.RegisterModel _registerModel = new APIClient.RegisterModel();
        string _roleSelect;
        List<string> _role = new List<string>() { "User", "Operator" };

        RadzenDataGrid<APIClient.IdentityUserDto> _profileGrid;
        IEnumerable<int> _pageSizeOptions = new int[] { 5, 10, 20, 30, 100, 200 };
        bool _showPagerSummary = true;
        string _pagingSummaryFormat = "Displaying page {0} of {1} <b>(total {2} records)</b>";

        private HttpClient ApiClient => _httpClientFactory.CreateClient("GiamSatAPI");

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            RefreshData();
        }

        async Task DeleteItem(string id, string userName)
        {
            if (_disposed) return;
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
                    if (_disposed) return;
                    _notificationService.Notify(new NotificationMessage()
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = res.Message,
                        Duration = 2000
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
                        Duration = 2000
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
                    Duration = 2000
                });

                return;
            }
        }

        async Task AddNewItem()
        {
            if (_disposed) return;
            var res = await _dialogService.OpenAsync<DialogCardPageAddNewUser>($"Tạo tài khoản",
                    new Dictionary<string, object>() { },
                    new DialogOptions() { Width = "500px", Height = "550px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });

            if (_disposed) return;
            if (res == "Success")
            {
                RefreshData();
            }
        }

        async Task EditUserRoles(APIClient.IdentityUserDto user)
        {
            if (_disposed) return;
            try
            {
                // Load available roles
                var allRoles = await ApiClient.GetFromJsonAsync<List<GiamSat.Models.IdentityRoleDto>>("api/permissions/roles");
                if (allRoles == null || _disposed) return;

                var availableRoleNames = allRoles.Select(r => r.Name).ToList();
                var currentRoles = user.Roles ?? new List<string>();

                // Build checkbox options — list of (RoleName, IsChecked)
                var roleSelections = availableRoleNames
                    .Select(r => new RoleSelection { Name = r, IsSelected = currentRoles.Contains(r) })
                    .ToList();

                var result = await _dialogService.OpenAsync<DialogEditUserRoles>(
                    $"Phân role: {user.UserName}",
                    new Dictionary<string, object>()
                    {
                        { "UserName", user.UserName },
                        { "RoleSelections", roleSelections }
                    },
                    new DialogOptions() { Width = "420px", Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true });

                if (_disposed) return;
                if (result is List<string> selectedRoles)
                {
                    var response = await ApiClient.PutAsJsonAsync($"api/permissions/users/{user.Id}/roles", selectedRoles);
                    if (response.IsSuccessStatusCode)
                    {
                        _notificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Thành công",
                            Detail = $"Đã cập nhật role cho {user.UserName}",
                            Duration = 2000
                        });
                        RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Lỗi",
                    Detail = ex.Message,
                    Duration = 3000
                });
            }
        }

        async Task ResetUserPassword(APIClient.IdentityUserDto user)
        {
            var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn Reset mật khẩu cho user: {user.UserName} về mặc định (123@456)?", "Reset mật khẩu", new ConfirmOptions()
            {
                OkButtonText = "Đồng ý",
                CancelButtonText = "Hủy",
                AutoFocusFirstElement = true,
            });

            if (confirm == true)
            {
                var res = await _authSerivce.ResetPassword(new ResetPasswordModel
                {
                    Username = user.UserName
                });

                if (res.Status == "Success")
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Thành công",
                        Detail = "Reset mật khẩu thành công. Mật khẩu mới mặc định là 123@456",
                        Duration = 5000
                    });
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Lỗi",
                        Detail = res.Message,
                        Duration = 5000
                    });
                }
            }
        }

        async Task RefreshData()
        {
            if (_disposed) return;
            try
            {
                var res = await ApiClient.GetFromJsonAsync<List<APIClient.IdentityUserDto>>("api/permissions/users");
                if (res != null && !_disposed)
                {
                    _users = res;
                    await _profileGrid.RefreshDataAsync();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 2000
                });
                return;
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
