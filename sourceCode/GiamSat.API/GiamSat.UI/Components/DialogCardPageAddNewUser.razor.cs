using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageAddNewUser
    {
        private RegisterModel _model = new RegisterModel();
        List<string> _roles = new List<string>() { "User", "Operator" };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _model.Email = "user@gmail.com";
        }

        async void Submit(RegisterModel arg)
        {
            var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn tạo user: {arg.UserName}", "Tạo user", new ConfirmOptions()
            {
                OkButtonText = "Yes",
                CancelButtonText = "No",
                AutoFocusFirstElement = true,
            });

            if (confirm == null || confirm == false) return;

            var res = await _authSerivce.RegisterUser(arg);

            if (res.Status == "Success")
            {
                _notificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = res.Message,
                    Duration = 2000
                });

                _dialogService.Close("Success");
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
    }
}
