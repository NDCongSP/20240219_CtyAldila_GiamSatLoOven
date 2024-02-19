using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Runtime.CompilerServices;

namespace GiamSat.UI.Components
{
    public partial class NangSuatView
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }


        [Parameter]
        public NangSuatModel NangSuat { get; set; }
        [Parameter]
        public string ChuongId { get; set; }

        private MudTheme _theme = new MudTheme();

        void Submit() => MudDialog.Close(DialogResult.Ok(true));
        void Cancel() => MudDialog.Cancel();
    }
}
