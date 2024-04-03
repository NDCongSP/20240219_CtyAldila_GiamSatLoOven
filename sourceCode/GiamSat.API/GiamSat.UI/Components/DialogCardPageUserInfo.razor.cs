using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Security.Claims;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageUserInfo
    {
        [Parameter] public Guid UserId { get; set; }

        UpdateModel _userModel = new UpdateModel();


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var authState = await _authSerivce.GetAuthenticationStateAsync();

            _userModel.Username = authState.User.Identity.Name;
            _userModel.Email = authState.User.FindFirst(ClaimTypes.Email).Value;
            var t = authState.User.FindFirst("emailTest").Value;
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
