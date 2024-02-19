using GiamSat.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json;

namespace GiamSat.UI.Pages
{
    public partial class SettingsChuong1
    {
        //tao bien Routing parameter. nay dùng để truyền thông số vào khi gọi trang
        [Parameter]
        public string ChuongId { get; set; }

        private MudTheme _theme = new MudTheme();

        //tạo các biến dùng cho tạo select chọn giai doạn
        private string stringValue { get; set; }
        private StepRun enumValue { get; set; } = StepRun.GiaiDoan1;
        public enum StepRun { GiaiDoan1 = 1, GiaiDoan2 = 2, GiaiDoan3 = 3, GiaiDoan4 = 4, GiaiDoan5 = 5, GiaiDoan6 = 6, GiaiDoan7 = 7, GiaiDoan8 = 8, GiaiDoan9 = 9, GiaiDoan10 = 10, }

        //các biến xử lý data
        APIClient.ChuongInfoModel _chuongInfo = new APIClient.ChuongInfoModel();
        APIClient.FT101 _ft101 = new APIClient.FT101();//lưu lại các thông tin chỉnh sửa, lịch sử cài đặt theo chu kỳ nuôi

        SettingsModel _chuongModel = new SettingsModel();
        EditFromSettingChuongModel _stepModel = new EditFromSettingChuongModel() { FromDate = 1, ToDate = 2, StaticFanRun = 3 };//model dùng để bind data editform
        bool success;

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                Guid chuongId = Guid.TryParse(ChuongId, out var value) ? value : Guid.Empty;
                var res = await _chuongInfoClient.GetByIdAsync(chuongId);

                success = res.Succeeded;

                if (res.Succeeded)
                {
                    _chuongInfo = null;
                    _chuongInfo = new APIClient.ChuongInfoModel();

                    _ft101 = null;
                    _ft101 = new APIClient.FT101();

                    _chuongModel = null;
                    _chuongModel = new SettingsModel();

                    _stepModel = null;
                    _stepModel = new EditFromSettingChuongModel();

                    _chuongInfo = res.Data;

                    #region Get thông tin của bảng chuongInfo lên để chuẩn bị data lưu vào bảng lịch sử cài đặt FT101
                    var resChuongInfo = await _chuongInfoClient.GetAllAsync();
                    if (resChuongInfo.Succeeded)
                    {
                        var d = resChuongInfo.Data.ToList();
                        GlobalVariable.FT101Details = null;
                        GlobalVariable.FT101Details = new List<SettingsModel>();
                        foreach (var item in d)
                        {
                            GlobalVariable.FT101Details.Add(JsonConvert.DeserializeObject<SettingsModel>(item.ConfigSettings));
                        }
                    }
                    #endregion

                    #region khoi tao thong tin cai dat cho chuong
                    if (string.IsNullOrEmpty(_chuongInfo.ConfigSettings) || _chuongInfo.ConfigSettings == "")
                    {
                        //get me nuoi. Luôn trả về 1 dòng dữ liệu, chính là mẻ nuôi hiện tại.
                        var chuKuNuoi = await _ft100Client.GetAllAsync();

                        int dayIndex = 0;
                        _chuongModel.Chuongid = chuongId;
                        _chuongModel.FT100Id = chuKuNuoi.Data.ToList()[0].Id;

                        for (int i = 1; i <= 10; i++)
                        {
                            _chuongModel.Steps.Add(new StepSettingsModel()
                            {
                                StepId = i,
                                FromDate = dayIndex,
                                ToDate = dayIndex + 1,
                                StaticFanRun = 0,
                                HightTemperature = 30,
                                Lowtemperature = 15,
                                HightFrequency = 50,
                                LowFrequency = 10,
                                TempRunFan1 = 20,
                                TempRunFan2 = 21,
                                TempRunFan3 = 22,
                                TempRunFan4 = 23,
                                TempRunCooler = 25
                            });

                            dayIndex += 2;
                        }

                        _chuongModel.GeneralSettings.Fan1 = 1;
                        _chuongModel.GeneralSettings.Fan2 = 2;
                        _chuongModel.GeneralSettings.Fan3 = 3;
                        _chuongModel.GeneralSettings.Fan4 = 4;
                        _chuongModel.GeneralSettings.TimeOnCooler = 2;
                        _chuongModel.GeneralSettings.TimeOffCooler = 1;
                        _chuongModel.GeneralSettings.TenChuong = _chuongInfo.TenChuong;
                        _chuongModel.GeneralSettings.NumIndex = (int)_chuongInfo.NumIndex;
                        _chuongModel.GeneralSettings.CurrentDay = 0;
                        _chuongModel.GeneralSettings.OffsetTemp = 0;
                        _chuongModel.GeneralSettings.DeadbandTemp = 0;
                        _chuongModel.GeneralSettings.ResetGiaiDoan = 0;

                        //năng suất
                        _chuongModel.NangSuat.ChuongId = chuongId;

                        _chuongInfo.ConfigSettings = JsonConvert.SerializeObject(_chuongModel);
                        _chuongInfo.FlagUpdate = 1;

                        await _chuongInfoClient.UpdateAsync(_chuongInfo);

                        #region Lưu vào bảng lịch sử của chu kỳ nuôi
                        try
                        {
                            var item = GlobalVariable.FT101Details.FirstOrDefault(x => x.Chuongid == _chuongInfo.Id);
                            item = _chuongModel;
                        }
                        catch
                        {
                            GlobalVariable.FT101Details.Remove(null);
                            GlobalVariable.FT101Details.Add(_chuongModel);
                        }

                        _ft101.FT100Id = _chuongModel.FT100Id;
                        _ft101.Details = JsonConvert.SerializeObject(GlobalVariable.FT101Details);
                        await _ft101Client.InsertAsync(_ft101);
                        #endregion
                    }
                    #endregion

                    _chuongModel = JsonConvert.DeserializeObject<SettingsModel>(_chuongInfo.ConfigSettings);

                    Console.WriteLine($"{_chuongInfo.ConfigSettings}");

                    #region lay ra thong so cai dat cho giai doan dau tien
                    var stepFirst = _chuongModel.Steps.FirstOrDefault();

                    _stepModel.ChuongId = _chuongModel.Chuongid;
                    _stepModel.TenChuong = _chuongInfo.TenChuong;
                    _stepModel.NumIndex = (int)_chuongInfo.NumIndex;
                    _stepModel.StepId = stepFirst.StepId;
                    _stepModel.FromDate = stepFirst.FromDate;
                    _stepModel.ToDate = stepFirst.ToDate;
                    _stepModel.StaticFanRun = stepFirst.StaticFanRun;
                    _stepModel.HightTemperature = stepFirst.HightTemperature;
                    _stepModel.Lowtemperature = stepFirst.Lowtemperature;
                    _stepModel.HightFrequency = stepFirst.HightFrequency;
                    _stepModel.LowFrequency = stepFirst.LowFrequency;
                    _stepModel.TempRunFan1 = stepFirst.TempRunFan1;
                    _stepModel.TempRunFan2 = stepFirst.TempRunFan2;
                    _stepModel.TempRunFan3 = stepFirst.TempRunFan3;
                    _stepModel.TempRunFan4 = stepFirst.TempRunFan4;
                    _stepModel.TempRunCooler = stepFirst.TempRunCooler;
                    _stepModel.Fan1 = _chuongModel.GeneralSettings.Fan1;
                    _stepModel.Fan2 = _chuongModel.GeneralSettings.Fan2;
                    _stepModel.Fan3 = _chuongModel.GeneralSettings.Fan3;
                    _stepModel.Fan4 = _chuongModel.GeneralSettings.Fan4;
                    _stepModel.TimeOnCooler = _chuongModel.GeneralSettings.TimeOnCooler;
                    _stepModel.TimeOffCooler = _chuongModel.GeneralSettings.TimeOffCooler;
                    _stepModel.CurrentDay = _chuongModel.GeneralSettings.CurrentDay;
                    _stepModel.OffsetTemp = _chuongModel.GeneralSettings.OffsetTemp;
                    _stepModel.DeadbandTemp = _chuongModel.GeneralSettings.DeadbandTemp;
                    _stepModel.ResetGiaiDoan = _chuongModel.GeneralSettings.ResetGiaiDoan;

                    _stepModel.SoLuongBanDau = _chuongModel.NangSuat.SoLuongBanDau;
                    _stepModel.SoLuongChet=_chuongModel.NangSuat.SoLuongChet;
                    _stepModel.SoLuongBiBenh = _chuongModel.NangSuat.SoLuongBiBenh;
                    _stepModel.SoLuongDangDieuTri = _chuongModel.NangSuat.SoLuongDangDieuTri;
                    _stepModel.TongKhoiLuongThucAn = _chuongModel.NangSuat.TongKhoiLuongThucAn;
                    _stepModel.KhoiLuongTren1Con = _chuongModel.NangSuat.KhoiLuongTren1Con;
                    #endregion
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Guid chuongId = Guid.TryParse(ChuongId, out var value) ? value : Guid.Empty;
                var res = await _chuongInfoClient.GetByIdAsync(chuongId);

                success = res.Succeeded;

                if (res.Succeeded)
                {
                    _chuongInfo = null;
                    _chuongInfo = new APIClient.ChuongInfoModel();

                    _ft101 = null;
                    _ft101 = new APIClient.FT101();

                    _chuongModel = null;
                    _chuongModel = new SettingsModel();

                    _stepModel = null;
                    _stepModel = new EditFromSettingChuongModel();

                    _chuongInfo = res.Data;

                    #region Get thông tin của bảng chuongInfo lên để chuẩn bị data lưu vào bảng lịch sử cài đặt FT101
                    var resChuongInfo = await _chuongInfoClient.GetAllAsync();
                    if (resChuongInfo.Succeeded)
                    {
                        var d = resChuongInfo.Data.ToList();
                        GlobalVariable.FT101Details = null;
                        GlobalVariable.FT101Details = new List<SettingsModel>();
                        foreach (var item in d)
                        {
                            GlobalVariable.FT101Details.Add(JsonConvert.DeserializeObject<SettingsModel>(item.ConfigSettings));
                        }
                    }
                    #endregion

                    #region khoi tao thong tin cai dat cho chuong
                    if (string.IsNullOrEmpty(_chuongInfo.ConfigSettings) || _chuongInfo.ConfigSettings == "")
                    {
                        //get me nuoi. Luôn trả về 1 dòng dữ liệu, chính là mẻ nuôi hiện tại.
                        var chuKuNuoi = await _ft100Client.GetAllAsync();

                        int dayIndex = 0;
                        _chuongModel.Chuongid = chuongId;
                        _chuongModel.FT100Id = chuKuNuoi.Data.ToList()[0].Id;

                        for (int i = 1; i <= 10; i++)
                        {
                            _chuongModel.Steps.Add(new StepSettingsModel()
                            {
                                StepId = i,
                                FromDate = dayIndex,
                                ToDate = dayIndex + 1,
                                StaticFanRun = 1,
                                HightTemperature = 30,
                                Lowtemperature = 15,
                                HightFrequency = 50,
                                LowFrequency = 10,
                                TempRunFan1 = 20,
                                TempRunFan2 = 21,
                                TempRunFan3 = 22,
                                TempRunFan4 = 23,
                                TempRunCooler = 25

                            });

                            dayIndex += 2;
                        }

                        _chuongModel.GeneralSettings.Fan1 = 1;
                        _chuongModel.GeneralSettings.Fan2 = 2;
                        _chuongModel.GeneralSettings.Fan3 = 3;
                        _chuongModel.GeneralSettings.Fan4 = 4;
                        _chuongModel.GeneralSettings.TimeOnCooler = 2;
                        _chuongModel.GeneralSettings.TimeOffCooler = 1;
                        _chuongModel.GeneralSettings.TenChuong = _chuongInfo.TenChuong;
                        _chuongModel.GeneralSettings.NumIndex = (int)_chuongInfo.NumIndex;
                        _chuongModel.GeneralSettings.CurrentDay = 0;
                        _chuongModel.GeneralSettings.OffsetTemp = 0;
                        _chuongModel.GeneralSettings.DeadbandTemp = 0;
                        _chuongModel.GeneralSettings.ResetGiaiDoan = 0;

                        //năng suất
                        _chuongModel.NangSuat.ChuongId = chuongId;

                        _chuongInfo.ConfigSettings = JsonConvert.SerializeObject(_chuongModel);
                        _chuongInfo.FlagUpdate = 1;

                        await _chuongInfoClient.UpdateAsync(_chuongInfo);

                        #region Lưu vào bảng lịch sử của chu kỳ nuôi
                        try
                        {
                            var item=GlobalVariable.FT101Details.FirstOrDefault(x=>x.Chuongid==_chuongInfo.Id);
                            item = _chuongModel;
                        }
                        catch
                        {
                            GlobalVariable.FT101Details.Remove(null);
                            GlobalVariable.FT101Details.Add(_chuongModel);
                        }

                        _ft101.FT100Id = _chuongModel.FT100Id;
                        _ft101.Details = JsonConvert.SerializeObject(GlobalVariable.FT101Details);
                        await _ft101Client.InsertAsync(_ft101);
                        #endregion
                    }
                    #endregion

                    _chuongModel = JsonConvert.DeserializeObject<SettingsModel>(_chuongInfo.ConfigSettings);

                    Console.WriteLine($"{_chuongInfo.ConfigSettings}");

                    #region lay ra thong so cai dat cho giai doan dau tien
                    var stepFirst = _chuongModel.Steps.FirstOrDefault();

                    _stepModel.ChuongId = _chuongModel.Chuongid;
                    _stepModel.TenChuong = _chuongInfo.TenChuong;
                    _stepModel.NumIndex = (int)_chuongInfo.NumIndex;
                    _stepModel.StepId = stepFirst.StepId;
                    _stepModel.FromDate = stepFirst.FromDate;
                    _stepModel.ToDate = stepFirst.ToDate;
                    _stepModel.StaticFanRun = stepFirst.StaticFanRun;
                    _stepModel.HightTemperature = stepFirst.HightTemperature;
                    _stepModel.Lowtemperature = stepFirst.Lowtemperature;
                    _stepModel.HightFrequency = stepFirst.HightFrequency;
                    _stepModel.LowFrequency = stepFirst.LowFrequency;
                    _stepModel.TempRunFan1 = stepFirst.TempRunFan1;
                    _stepModel.TempRunFan2 = stepFirst.TempRunFan2;
                    _stepModel.TempRunFan3 = stepFirst.TempRunFan3;
                    _stepModel.TempRunFan4 = stepFirst.TempRunFan4;
                    _stepModel.TempRunCooler = stepFirst.TempRunCooler;
                    _stepModel.Fan1 = _chuongModel.GeneralSettings.Fan1;
                    _stepModel.Fan2 = _chuongModel.GeneralSettings.Fan2;
                    _stepModel.Fan3 = _chuongModel.GeneralSettings.Fan3;
                    _stepModel.Fan4 = _chuongModel.GeneralSettings.Fan4;
                    _stepModel.TimeOnCooler = _chuongModel.GeneralSettings.TimeOnCooler;
                    _stepModel.TimeOffCooler = _chuongModel.GeneralSettings.TimeOffCooler;
                    _stepModel.CurrentDay = _chuongModel.GeneralSettings.CurrentDay;
                    _stepModel.OffsetTemp = _chuongModel.GeneralSettings.OffsetTemp;
                    _stepModel.DeadbandTemp = _chuongModel.GeneralSettings.DeadbandTemp;
                    _stepModel.ResetGiaiDoan = _chuongModel.GeneralSettings.ResetGiaiDoan;

                    _stepModel.SoLuongBanDau = _chuongModel.NangSuat.SoLuongBanDau;
                    _stepModel.SoLuongChet = _chuongModel.NangSuat.SoLuongChet;
                    _stepModel.SoLuongBiBenh = _chuongModel.NangSuat.SoLuongBiBenh;
                    _stepModel.SoLuongDangDieuTri = _chuongModel.NangSuat.SoLuongDangDieuTri;
                    _stepModel.TongKhoiLuongThucAn = _chuongModel.NangSuat.TongKhoiLuongThucAn;
                    _stepModel.KhoiLuongTren1Con = _chuongModel.NangSuat.KhoiLuongTren1Con;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
                return;
            }
        }

        private async void OnValidSubmit(EditContext context)
        {
            try
            {
                var step = _chuongModel.Steps.FirstOrDefault(x => x.StepId == _stepModel.StepId);
                if (step != null)
                {
                    step.StepId = _stepModel.StepId;
                    step.FromDate = _stepModel.FromDate;
                    step.ToDate = _stepModel.ToDate;
                    step.StaticFanRun = _stepModel.StaticFanRun;
                    step.HightTemperature = _stepModel.HightTemperature;
                    step.Lowtemperature = _stepModel.Lowtemperature;
                    step.HightFrequency = _stepModel.HightFrequency;
                    step.LowFrequency = _stepModel.LowFrequency;
                    step.TempRunFan1 = _stepModel.TempRunFan1;
                    step.TempRunFan2 = _stepModel.TempRunFan2;
                    step.TempRunFan3 = _stepModel.TempRunFan3;
                    step.TempRunFan4 = _stepModel.TempRunFan4;
                    step.TempRunCooler = _stepModel.TempRunCooler;
                }
                _chuongModel.GeneralSettings.TenChuong = _stepModel.TenChuong;
                _chuongModel.GeneralSettings.NumIndex = _stepModel.NumIndex;
                _chuongModel.GeneralSettings.Fan1 = _stepModel.Fan1;
                _chuongModel.GeneralSettings.Fan2 = _stepModel.Fan2;
                _chuongModel.GeneralSettings.Fan3 = _stepModel.Fan3;
                _chuongModel.GeneralSettings.Fan4 = _stepModel.Fan4;
                _chuongModel.GeneralSettings.TimeOnCooler = _stepModel.TimeOnCooler;
                _chuongModel.GeneralSettings.TimeOffCooler = _stepModel.TimeOffCooler;
                _chuongModel.GeneralSettings.CurrentDay = _stepModel.CurrentDay;
                _chuongModel.GeneralSettings.OffsetTemp = _stepModel.OffsetTemp;
                _chuongModel.GeneralSettings.DeadbandTemp = _stepModel.DeadbandTemp;
                _chuongModel.GeneralSettings.ResetGiaiDoan = _stepModel.ResetGiaiDoan;

                _chuongModel.NangSuat.SoLuongBanDau = _stepModel.SoLuongBanDau;
                _chuongModel.NangSuat.SoLuongChet = _stepModel.SoLuongChet;
                _chuongModel.NangSuat.SoLuongBiBenh = _stepModel.SoLuongBiBenh;
                _chuongModel.NangSuat.SoLuongDangDieuTri = _stepModel.SoLuongDangDieuTri;
                _chuongModel.NangSuat.TongKhoiLuongThucAn = _stepModel.TongKhoiLuongThucAn;
                _chuongModel.NangSuat.KhoiLuongTren1Con = _stepModel.KhoiLuongTren1Con;

                _chuongInfo.ConfigSettings = JsonConvert.SerializeObject(_chuongModel);
                _chuongInfo.TenChuong = _stepModel.TenChuong;
                _chuongInfo.NumIndex = _stepModel.NumIndex;
                _chuongInfo.FlagUpdate = 1;

                var res = await _chuongInfoClient.UpdateAsync(_chuongInfo);
                await _displayRealtimeClient.UpdateTenChuongAsync(new APIClient.DisplayRealTimeModel()
                {
                    ChuongId = _chuongInfo.Id,
                    TenChuong = _chuongInfo.TenChuong,
                    NumIndex = (int)_chuongInfo.NumIndex,
                    HightTemperature = _stepModel.HightTemperature,
                    Lowtemperature = _stepModel.Lowtemperature,
                });

                #region Lưu vào bảng lịch sử của chu kỳ nuôi
                //var itemUpdate = GlobalVariable.FT101Details.FirstOrDefault(x => x.Chuongid == _chuongInfo.Id);
                //itemUpdate = _chuongModel;

                _ft101.FT100Id = _chuongModel.FT100Id;
                _ft101.Details = JsonConvert.SerializeObject(_chuongModel);
                await _ft101Client.InsertAsync(_ft101);
                #endregion

                //success = res.Succeeded;

                _snackBar.Add("Update successfull", Severity.Success);

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
            }
        }

        private void OnInvalidSubmit(EditContext context)
        {
            success = false;
            _snackBar.Add($"Invalid data", Severity.Error);
            StateHasChanged();
        }

        private void OnSelectedValuesChanged(IEnumerable<StepRun> values)
        {
            Console.WriteLine(string.Join("SELECTED value:\n", values));
            #region lay ra thong so cai dat cho giai doan dau tien
            var step = values.FirstOrDefault();
            var stepFirst = _chuongModel.Steps.FirstOrDefault(x => x.StepId == (int)step);

            _stepModel.ChuongId = _chuongModel.Chuongid;
            _stepModel.TenChuong = _chuongInfo.TenChuong;
            _stepModel.NumIndex = (int)_chuongInfo.NumIndex;
            _stepModel.StepId = stepFirst.StepId;
            _stepModel.FromDate = stepFirst.FromDate;
            _stepModel.ToDate = stepFirst.ToDate;
            _stepModel.StaticFanRun = stepFirst.StaticFanRun;
            _stepModel.HightTemperature = stepFirst.HightTemperature;
            _stepModel.Lowtemperature = stepFirst.Lowtemperature;
            _stepModel.HightFrequency = stepFirst.HightFrequency;
            _stepModel.LowFrequency = stepFirst.LowFrequency;
            _stepModel.TempRunFan1 = stepFirst.TempRunFan1;
            _stepModel.TempRunFan2 = stepFirst.TempRunFan2;
            _stepModel.TempRunFan3 = stepFirst.TempRunFan3;
            _stepModel.TempRunFan4 = stepFirst.TempRunFan4;
            _stepModel.TempRunCooler = stepFirst.TempRunCooler;
            _stepModel.Fan1 = _chuongModel.GeneralSettings.Fan1;
            _stepModel.Fan2 = _chuongModel.GeneralSettings.Fan2;
            _stepModel.Fan3 = _chuongModel.GeneralSettings.Fan3;
            _stepModel.Fan4 = _chuongModel.GeneralSettings.Fan4;
            _stepModel.TimeOnCooler = _chuongModel.GeneralSettings.TimeOnCooler;
            _stepModel.TimeOffCooler = _chuongModel.GeneralSettings.TimeOffCooler;
            #endregion

            StateHasChanged();
        }
    }
}
