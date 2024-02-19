using GiamSat.APIClient;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.Interface.Pages
{
    public partial class Login
    {
        private MudTheme _theme = new MudTheme();
        APIClient.LoginResult login;
        private string token;
        private LoginModel _model = new LoginModel();

        bool _passwordVisibility = false;
        InputType _passwordInput = InputType.Password;
        string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void OnTogglePasswordVisibility()
        {
            if (_passwordVisibility)
            {
                _passwordVisibility = false;
                _passwordInput = InputType.Password;
                _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            }
            else
            {
                _passwordVisibility = true;
                _passwordInput = InputType.Text;
                _passwordInputIcon = Icons.Material.Filled.Visibility;
            }
        }

        private async Task HandleLogin()
        {
            var success = await _authSerivce.LoginAsync(_model);

            if (success != null)
            {
                token = success.Token;
                login = success;
                
                _snackBar.Add("Login successfull", Severity.Success);
                //await InvokeAsync(StateHasChanged);
                //StateHasChanged();
                _navigation.NavigateTo("/");
            }
            else
            {
                _snackBar.Add("Login fail", Severity.Error);

                //StateHasChanged();
            }
        }
    }
}
