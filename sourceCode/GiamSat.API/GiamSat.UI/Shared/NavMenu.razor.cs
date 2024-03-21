using Microsoft.AspNetCore.Components;

namespace GiamSat.UI.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public bool sidebarExpanded { get; set; } = true;
    }
}
