using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Security.Claims;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageUserInfo
    {
        [Parameter] public Guid UserId { get; set; }
        [CascadingParameter] RadzenStack? stack { get; set; }
        private UpdateModel _userModel = new UpdateModel();
        private bool _showOldPassword;
        private bool _showNewPassword;
        private bool _showRepeatPassword;


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var authState = await _authSerivce.GetAuthenticationStateAsync();

            _userModel.Username = authState.User.Identity.Name;
            _userModel.Email = authState.User.FindFirst(ClaimTypes.Email)?.Value;
        }

        async void Submit(UpdateModel arg)
        {
            try
            {
                //var res = await _authSerivce.CheckUser(new LoginModel() { Username = arg.Username, Password = arg.OldPassword });
                var res = await _authSerivce.UpdateUser(arg);

                if (res.Status == "Success")
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = res.Status,
                        Detail = res.Message,
                        Duration = 2000
                    });

                    _dialogService.Close("Success");
                }
                else
                {
                    _notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = res.Status,
                        Detail = res.Message,
                        Duration = 2000
                    });
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Login fail: {ex.Message}",
                    Duration = 2000
                });

                return;
            }
        }
    }
}
