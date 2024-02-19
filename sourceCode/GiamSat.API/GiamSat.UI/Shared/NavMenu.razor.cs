using GiamSat.APIClient;
using Microsoft.AspNetCore.Components.Web;

namespace GiamSat.UI.Shared
{
    public partial class NavMenu
    {
        private List<ChuongInfoModel> chuongInfo =new List<ChuongInfoModel>();

        private string linkC;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var res = await _chuongInfoClient.GetAllAsync();

                if (res.Succeeded)
                {
                    chuongInfo = res.Data.OrderBy(x => x.NumIndex).ToList();
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, MudBlazor.Severity.Error);
            }
        }

        async void OnclickLogout(MouseEventArgs args)
        {
            await _authSerivce.LogoutAsync();
        }
    }
}
