using BlazorBootstrap;
using Blazorise;
using GiamSat.Models;
using MudBlazor;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Timers;

namespace GiamSat.UI.Pages
{
    public partial class Trend
    {
        private LineChart lineChart = default!;
        private LineChartOptions lineChartOptions = default!;
        private ChartData chartData = default!;

        private int datasetsCount;
        private int labelsCount;

        private Random random = new();

        private List<APIClient.RealtimeDisplayModel>? _dataFromDB;
        private RealtimeList? _displayRealtime;

        private System.Timers.Timer _timer;
        private bool _timerTick = false;

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitialChart();
                await lineChart.InitializeAsync(chartData: chartData, chartOptions: lineChartOptions);
                ////hiển thị value tại mỗi điểm
                //await lineChart.InitializeAsync(chartData: chartData, chartOptions: lineChartOptions, plugins: new string[] { "ChartDataLabels" });

                #region Timer refresh data
                _timer = new System.Timers.Timer(GlobalVariable.ChartRefreshInterval);
                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
                #endregion
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task InitialChart()
        {
            var colors = ColorBuilder.CategoricalTwelveColors;
            var labels = new List<string> { DateTime.Now.ToString("HH:mm:ss") };
            var datasets = new List<IChartDataset>();


            lineChartOptions = new();
            lineChartOptions.Responsive = true;
            lineChartOptions.Interaction = new Interaction { Mode = InteractionMode.Index };

            lineChartOptions.Scales.X.Title.Text = "Thời gian";
            lineChartOptions.Scales.X.Title.Display = true;

            lineChartOptions.Scales.Y.Title.Text = "Nhiệt độ (oC)";
            lineChartOptions.Scales.Y.Title.Display = true;

            lineChartOptions.Plugins.Title.Text = "Đồ thị nhiệt độ";
            lineChartOptions.Plugins.Title.Display = true;

            //// datalabels
            //lineChartOptions.Plugins.Datalabels.Color = "white";
            //lineChartOptions.Plugins.Datalabels.Font = new LineChartDataLabelsFont() { Weight = "Size.Small" };

            try
            {
                var res = await _realtimeDisplayClient.GetAllAsync().ConfigureAwait(true);

                if (res.Succeeded)
                {
                    _dataFromDB = res.Data.ToList();

                    if (_dataFromDB == null && _dataFromDB.Count <= 0)
                    {
                        _snackBar.Add("Data Null", Severity.Warning);
                        return;
                    }

                    _displayRealtime = JsonConvert.DeserializeObject<RealtimeList>(_dataFromDB.FirstOrDefault().DisplayData);

                    int index = 0;

                    foreach (var item in _displayRealtime)
                    {
                        var dataset = new LineChartDataset
                        {
                            Label = $"{item.DisplayRealtime.TenChuong}",
                            Data = new List<double> { item.DisplayRealtime.Temperature },
                            BackgroundColor = new List<string> { colors[index] },
                            BorderColor = new List<string> { colors[index] },
                            BorderWidth = new List<double> { 2 },
                            HoverBorderWidth = new List<double> { 4 },
                            PointBackgroundColor = new List<string> { colors[index] },
                            PointRadius = new List<int> { 3 }, // hide points
                            PointHoverRadius = new List<int> { 4 },
                            PointStyle = new List<string> { "diamond" },//diamond
                            // datalabels
                            //Datalabels = new() { Align = "end", Anchor = "end" },

                        };
                        datasets.Add(dataset);
                        index += 1;
                    }
                }
                else
                {
                    foreach (var item in res.Messages)
                    {
                        _snackBar.Add(item, Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
            }

            #region add datasheet
            //var dataset1 = new LineChartDataset
            //{
            //    Label = "Windows",
            //    Data = new List<double> { 7265791, 5899643, 6317759, 6315641, 5338211, 8496306, 7568556, 8538933, 8274297, 8657298, 7548388, 7764845 },
            //    BackgroundColor = new List<string> { colors[0] },
            //    BorderColor = new List<string> { colors[0] },
            //    BorderWidth = new List<double> { 2 },
            //    HoverBorderWidth = new List<double> { 4 },
            //    PointBackgroundColor = new List<string> { colors[0] },
            //    PointRadius = new List<int> { 0 }, // hide points
            //    PointHoverRadius = new List<int> { 4 },

            //    // datalabels
            //    Datalabels = new() { Align = "end", Anchor = "end"},

            //};
            //datasets.Add(dataset1);

            //var dataset2 = new LineChartDataset
            //{
            //    Label = "macOS",
            //    Data = new List<double> { 1809499, 1816642, 2122410, 1809499, 1850793, 1846743, 1954797, 2391313, 1983430, 2469918, 2633303, 2821149 },
            //    BackgroundColor = new List<string> { colors[1] },
            //    BorderColor = new List<string> { colors[1] },
            //    BorderWidth = new List<double> { 2 },
            //    HoverBorderWidth = new List<double> { 4 },
            //    PointBackgroundColor = new List<string> { colors[1] },
            //    PointRadius = new List<int> { 0 }, // hide points
            //    PointHoverRadius = new List<int> { 4 }
            //};
            //datasets.Add(dataset2);

            //var dataset3 = new LineChartDataset
            //{
            //    Label = "Other",
            //    Data = new List<double> { 1081241, 1100363, 1118136, 1073255, 1120315, 1395736, 1488788, 1489466, 1489947, 1414739, 1735811, 1820171 },
            //    BackgroundColor = new List<string> { colors[2] },
            //    BorderColor = new List<string> { colors[2] },
            //    BorderWidth = new List<double> { 2 },
            //    HoverBorderWidth = new List<double> { 4 },
            //    PointBackgroundColor = new List<string> { colors[2] },
            //    PointRadius = new List<int> { 0 }, // hide points
            //    PointHoverRadius = new List<int> { 4 }
            //};
            //datasets.Add(dataset3);
            #endregion

            chartData = new ChartData
            {
                Labels = labels,
                Datasets = datasets
            };
        }

        private async void RefreshData(object? sender, ElapsedEventArgs e)
        {
            try
            {
                await AddDataAsync();

                _timerTick = false;

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
            }
            catch { _timerTick = false; }
        }

        private async Task AddDatasetAsync()
        {
            if (chartData is null || chartData.Datasets is null) return;

            var chartDataset = GetLineChartDataset();
            chartData = await lineChart.AddDatasetAsync(chartData, chartDataset, lineChartOptions);
        }

        private async Task AddDataAsync()
        {
            try
            {
                if (!_timerTick)
                {
                    _timerTick = true;

                    if (chartData is null || chartData.Datasets is null)
                    {
                        return;
                    }

                    //add data to chart
                    var data = new List<IChartDatasetData>();

                    var res = await _realtimeDisplayClient.GetAllAsync().ConfigureAwait(true);

                    if (res.Succeeded)
                    {
                        _dataFromDB = res.Data.ToList();

                        if (_dataFromDB == null && _dataFromDB.Count <= 0)
                        {
                            _snackBar.Add("Data Null", Severity.Warning);
                            return;
                        }

                        _displayRealtime = JsonConvert.DeserializeObject<RealtimeList>(_dataFromDB.FirstOrDefault().DisplayData);

                        //add data to chart
                        foreach (var item in _displayRealtime)
                        {
                            foreach (var dataset in chartData.Datasets)
                            {
                                if (dataset is LineChartDataset lineChartDataset)
                                {
                                    //var ds = dataset as LineChartDataset;   

                                    if (item.DisplayRealtime.TenChuong == lineChartDataset.Label)
                                    {
                                        var count = lineChartDataset.Data.Count;

                                        if (count > GlobalVariable.ChartPointNum)
                                        {
                                            lineChartDataset.Data.RemoveAt(0);
                                        }

                                        data.Add(new LineChartDatasetData(lineChartDataset.Label, item.DisplayRealtime.Temperature));

                                        break;
                                    }
                                }
                            }
                        }

                        chartData = await lineChart.AddDataAsync(chartData, DateTime.Now.ToString("HH:mm:ss"), data);
                    }


                    //add data to chart
                    //var data = new List<IChartDatasetData>();
                    //foreach (var dataset in chartData.Datasets)
                    //{
                    //    if (dataset is LineChartDataset lineChartDataset)
                    //    {
                    //        //var ds = dataset as LineChartDataset;                         

                    //        var count = lineChartDataset.Data.Count;
                    //        Debug.WriteLine($"Count point: {count}");

                    //        if (count > 10)
                    //        {
                    //            lineChartDataset.Data.RemoveAt(0);
                    //        }
                    //        data.Add(new LineChartDatasetData(lineChartDataset.Label, random.Next(30)));
                    //    }
                    //}

                    if (chartData.Labels.Count > GlobalVariable.ChartPointNum)
                    {
                        chartData.Labels.RemoveAt(0);

                        await lineChart.UpdateAsync(chartData, lineChartOptions);
                    }

                    //await lineChart.UpdateAsync(chartData, lineChartOptions);

                    await lineChart.AddDataAsync(chartData, DateTime.Now.ToString("HH:mm:ss"), data);
                }
            }
            catch { }
        }

        #region Data Preparation

        private LineChartDataset GetLineChartDataset()
        {
            var c = ColorBuilder.CategoricalTwelveColors[datasetsCount].ToColor();

            datasetsCount += 1;

            return new LineChartDataset
            {
                Label = $"Team {datasetsCount}",
                Data = GetRandomData(),
                BackgroundColor = new List<string> { c.ToRgbString() },
                BorderColor = new List<string> { c.ToRgbString() },
                BorderWidth = new List<double> { 2 },
                HoverBorderWidth = new List<double> { 4 },
                PointBackgroundColor = new List<string> { c.ToRgbString() },
                PointRadius = new List<int> { 0 }, // hide points
                PointHoverRadius = new List<int> { 4 }
            };
        }

        private List<double> GetRandomData()
        {
            var data = new List<double>();
            for (var index = 0; index < labelsCount; index++)
            {
                data.Add(random.Next(200));
            }

            return data;
        }

        private string GetNextDataLabel()
        {
            labelsCount += 1;
            return $"Day {labelsCount}";
        }

        #endregion Data Preparation

    }
}
