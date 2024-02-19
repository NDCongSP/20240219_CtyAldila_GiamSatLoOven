using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.Interface.Pages
{
    public partial class SettingsChuong1
    {
        private string stringValue { get; set; }
        private StepRun enumValue { get; set; } = StepRun.GiaiDoan1;
        public enum StepRun { GiaiDoan1, GiaiDoan2, GiaiDoan3, GiaiDoan4, GiaiDoan5, GiaiDoan6, GiaiDoan7, GiaiDoan8, GiaiDoan9, GiaiDoan10, }

        APIClient.ChuongInfoModel chuongInfo = new APIClient.ChuongInfoModel();
        SettingsModel chuongModel = new SettingsModel();
        SettingBindingModel stepModel = new SettingBindingModel() { FromDate = 1, ToDate = 2, StaticFanRun = 3 };
        bool success;

        protected override async Task OnInitializedAsync()
        {
            var id = Guid.TryParse("E50409D5-AA02-4166-8F53-C5DDB304EB50", out Guid value) ? value : Guid.Empty;
            var res = await _chuongInfoClient.GetByIdAsync(id);

            success = res.Succeeded;

            if (res.Succeeded)
            {
                chuongInfo = res.Data;

                chuongModel = JsonConvert.DeserializeObject<SettingsModel>(chuongInfo.ConfigSettings);

                Console.WriteLine($"{chuongInfo.ConfigSettings}");
            }
        }

        private async void OnValidSubmit(EditContext context)
        {
            for (int i = 1; i <= 10; i++)
            {
                chuongModel.Steps.Add(new StepSettingsModel()
                {
                    StepId = i,
                    FromDate = 0,
                    ToDate = 1,
                    StaticFanRun = 2,
                    HightTemperature = 30,
                    Lowtemperature = 15,
                    HightFrequency = 50,
                    LowFrequency = 10,
                    TempRunFan1 = 20,
                    TempRunFan2 = 21,
                    TempRunFan3 = 22,
                    TempRunFan4 = 13,
                    TempRunCooler = 25
                });
            }

            chuongModel.GeneralSettings.Fan1 = 1;
            chuongModel.GeneralSettings.Fan2 = 2;
            chuongModel.GeneralSettings.Fan3 = 3;
            chuongModel.GeneralSettings.Fan4 = 4;
            chuongModel.GeneralSettings.TimeOnCooler = 2;
            chuongModel.GeneralSettings.TimeOffCooler = 1;
            chuongModel.GeneralSettings.TenChuong = "Chuồng 1";
            chuongModel.GeneralSettings.NumIndex = 1;

            chuongInfo.ConfigSettings = JsonConvert.SerializeObject(chuongModel);
            chuongInfo.TenChuong = chuongModel.GeneralSettings.TenChuong;
            chuongInfo.NumIndex = chuongModel.GeneralSettings.NumIndex;

            var res = await _chuongInfoClient.UpdateAsync(chuongInfo);

            success = res.Succeeded;

            _snackBar.Add("Update successfull", Severity.Success);
            StateHasChanged();
        }

        private void OnInvalidSubmit(EditContext context)
        {
            success = false;
            _snackBar.Add($"Invalid data", Severity.Error);
            StateHasChanged();
        }
    }
}
