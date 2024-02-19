using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.Interface.Shared
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
