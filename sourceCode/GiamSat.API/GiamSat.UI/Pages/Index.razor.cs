using DocumentFormat.OpenXml.Drawing;

namespace GiamSat.UI.Pages
{
    public partial class Index
    {
        protected override async Task OnInitializedAsync()
        {
            var res = await _ft01Client.GetAllAsync();
        }
    }
}
