using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace GiamSat.UI.Components
{
    public partial class DialogRevoConfig
    {
        [Parameter]
        public RevoConfigModel Model { get; set; } = new RevoConfigModel();

        [Parameter]
        public bool IsEdit { get; set; } = false;

        [Inject] private DialogService DialogService { get; set; } = default!;

        private RevoConfigModel _model = new RevoConfigModel();
        private bool _isEdit = false;

        protected override void OnParametersSet()
        {
            _model = new RevoConfigModel
            {
                Id = Model.Id,
                Name = Model.Name,
                Path = Model.Path,
                ConstringAccessDb = Model.ConstringAccessDb,
                Pulse_Rev = Model.Pulse_Rev
            };
            _isEdit = IsEdit;
        }

        void Submit(RevoConfigModel arg)
        {
            DialogService.Close(arg);
        }
    }
}
