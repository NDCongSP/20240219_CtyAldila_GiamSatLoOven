using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

namespace GiamSat.UI.Pages
{
    public partial class Login
    {
        APIClient.LoginResult login;
        private string token;
        async void OnLogin(LoginArgs args)
        {
            Console.WriteLine($"Username: {args.Username} and password: {args.Password}");
            try
            {
                var success = await _authSerivce.LoginAsync(new LoginModel()
                {
                    Username = args.Username,
                    Password = args.Password
                });

                if (success != null)
                {
                    token = success.Token;
                    login = success;

                    _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Successfull", Detail = "Login OK", Duration = 4000 });
                    //await InvokeAsync(StateHasChanged);
                    //StateHasChanged();

                    _navigation.NavigateTo("/");
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = "Login fail", Duration = 4000 });

                    StateHasChanged();
                }
            }
            catch
            {
                _notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Error", Detail = "Login fail", Duration = 4000 });

                StateHasChanged();
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
