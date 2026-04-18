using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GiamSat.APIClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

namespace GiamSat.UI.Pages
{
    //[AllowAnonymous]
    public partial class Login
    {
        private LoginModel _loginModel = new();
        private bool _showPassword;
        APIClient.LoginResult login;
        private string token;

        async Task OnLogin()
        {
            Console.WriteLine($"Username: {_loginModel.Username}");
            try
            {
                var success = await _authSerivce.LoginAsync(_loginModel);

                if (success != null)
                {
                    token = success.Token;
                    login = success;

                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Thành công",
                        Detail = "Đăng nhập thành công",
                        Duration = 2000
                    });

                    _navigation.NavigateTo("/");
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Lỗi",
                        Detail = "Đăng nhập thất bại",
                        Duration = 2000
                    });
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Lỗi",
                    Detail = $"Lỗi đăng nhập: {ex.Message}",
                    Duration = 2000
                });
            }
        }

        void OnRegister(string name)
        {
            Console.WriteLine($"{name} -> Register");
        }

        void OnResetPassword(string value, string name)
        {
            Console.WriteLine($"{name} -> ResetPassword for user: {value}");
        }
    }
}
