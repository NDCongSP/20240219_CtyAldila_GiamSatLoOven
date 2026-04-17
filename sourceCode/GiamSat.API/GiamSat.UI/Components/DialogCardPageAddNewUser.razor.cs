using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System.Net.Http.Json;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPageAddNewUser : IDisposable
    {
        private bool _disposed;
        private RegisterModel _model = new RegisterModel();
        List<string> _roles = new List<string>();

        private HttpClient ApiClient => _httpClientFactory.CreateClient("GiamSatAPI");

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _model.Email = "user@gmail.com";

            var roles = await ApiClient.GetFromJsonAsync<List<GiamSat.Models.IdentityRoleDto>>("api/permissions/roles");
            if (roles != null && !_disposed)
            {
                _roles = roles.Select(x => x.Name).ToList();
            }
        }

        async Task Submit(RegisterModel arg)
        {
            if (_disposed) return;
            var confirm = await _dialogService.Confirm($"Bạn chắc chắn muốn tạo user: {arg.UserName}", "Tạo user", new ConfirmOptions()
            {
                OkButtonText = "Yes",
                CancelButtonText = "No",
                AutoFocusFirstElement = true,
            });

            if (_disposed || confirm == null || confirm == false) return;

            var res = await _authSerivce.RegisterUser(arg);

            if (_disposed) return;

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

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
