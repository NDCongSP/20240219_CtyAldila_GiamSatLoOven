using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GiamSat.UI.Shared
{
    public partial class MainLayout : IDisposable
    {
        private MudTheme _theme = new MudTheme()
        {
            Typography = new Typography()
            {
                H6 = new H6()
                {
                    FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                    FontSize = "0.8rem",
                    FontWeight = 500,
                    LineHeight = 1.6,
                    LetterSpacing = ".0075em"
                }
            }
        };
        private bool _isDarkMode = false;

        bool _drawerOpen = false;

        void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }
        async void OnClockLogout(MouseEventArgs args)
        {
            await _authSerivce.LogoutAsync();
        }

        protected override void OnInitialized()
        {
            _interceptor.RegisterEvent();
            base.OnInitialized();
        }

        public void Dispose()
        {
            _interceptor.DisposeEvent();
        }

        private async void OnThemeClick()
        {
            if (_isDarkMode)
            {
                _isDarkMode = false;
            }
            else
            {
                _isDarkMode = true;
            }
        }
    }
}
