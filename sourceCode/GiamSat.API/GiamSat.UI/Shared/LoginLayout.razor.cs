namespace GiamSat.UI.Shared
{
    public partial class LoginLayout
    {
        bool _drawerOpen = true;

        void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        void BtnMenuItemProfileClick()
        {
            _navigation.NavigateTo("/login");
        }

        void BtnMenuItemSignOutClick()
        {
            //_authService.Logout();
            _navigation.NavigateTo("/login");
        }
    }
}
