using Microsoft.AspNetCore.Components.Web;
using System;
using System.Linq;

namespace GiamSat.Interface.Shared
{
    public partial class MainLayout
    {
        bool _drawerOpen = true;

        void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }
        async void  OnClockLogout(MouseEventArgs args)
        {
            await _authSerivce.LogoutAsync();
        }
    }
}
