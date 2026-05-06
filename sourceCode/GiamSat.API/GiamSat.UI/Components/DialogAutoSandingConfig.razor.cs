using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;

namespace GiamSat.UI.Components
{
    public partial class DialogAutoSandingConfig
    {
        [Parameter] public FT14_TipOdFreq Model { get; set; } = new();
        [Parameter] public bool IsEdit { get; set; } = false;

        private FT14_TipOdFreq _model = new();
        private bool _isEdit;

        protected override void OnParametersSet()
        {
            _isEdit = IsEdit;
            // Clone để tránh sửa trực tiếp vào list
            _model = new FT14_TipOdFreq
            {
                Id                   = Model.Id,
                Actived              = Model.Actived,
                CreatedAt            = Model.CreatedAt,
                CreatedMachine       = Model.CreatedMachine,
                PartName             = Model.PartName,
                FreqTarget           = Model.FreqTarget,
                Set_Freq_Offset_Low  = Model.Set_Freq_Offset_Low,
                Set_Freq_Offset_Hight= Model.Set_Freq_Offset_Hight,
                Formula_F            = Model.Formula_F,
                A                    = Model.A,
                B                    = Model.B,
                C                    = Model.C,
                D                    = Model.D,
                TipOdLength_1        = Model.TipOdLength_1,
                Diam_LL_1            = Model.Diam_LL_1,
                Diam_UL_1            = Model.Diam_UL_1,
                TipOdLength_2        = Model.TipOdLength_2,
                Diam_LL_2            = Model.Diam_LL_2,
                Diam_UL_2            = Model.Diam_UL_2,
                TipOdLength_3        = Model.TipOdLength_3,
                Diam_LL_3            = Model.Diam_LL_3,
                Diam_UL_3            = Model.Diam_UL_3,
            };
        }

        private void Submit(FT14_TipOdFreq arg)
        {
            _dialogService.Close(arg);
        }
    }
}
