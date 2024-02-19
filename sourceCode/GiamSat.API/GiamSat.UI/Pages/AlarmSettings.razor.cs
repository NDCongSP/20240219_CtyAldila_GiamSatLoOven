using GiamSat.APIClient;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace GiamSat.UI.Pages
{
    public partial class AlarmSettings
    {
        private AlarmSettingsModel _alarmSettings;

        private MudTheme _theme = new MudTheme();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var res = await _alarmSettingsClient.GetAllAsync();
                if (res != null && res.Succeeded)
                {
                    _alarmSettings = (AlarmSettingsModel)res.Data.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, MudBlazor.Severity.Error);
                return;
            }
        }

        private async void OnValidSubmit(EditContext context)
        {
            try
            {
                var res = await _alarmSettingsClient.UpdateAsync(_alarmSettings);

                if (res.Succeeded)
                {
                    _snackBar.Add("Cập nhật thành công.", Severity.Success);
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
                return;
            }
        }

        private void OnInvalidSubmit(EditContext context)
        {
            _snackBar.Add($"Invalid data", Severity.Error);
            StateHasChanged();
        }
    }
}
