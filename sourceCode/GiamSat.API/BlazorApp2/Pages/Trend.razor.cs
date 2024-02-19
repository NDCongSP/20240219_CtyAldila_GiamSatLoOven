using AntDesign.Charts;
using Title = AntDesign.Charts.Title;
using System.Diagnostics;
using System;
using System.Reflection.Emit;
using System.Timers;
using AntDesign;

namespace BlazorApp2.Pages
{
    public partial class Trend
    {
        IChartComponent chart1;
        IChartComponent chart5;

        Line lineChart = new Line();

        private System.Timers.Timer _timer;
        private bool _timerTick = false;

        protected override async Task OnInitializedAsync()
        {
            //await chart1.ChangeData(data1);


            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                lineChart = new Line();
                lineChart.Config = config3;
                lineChart.Data = data3;
                

                #region Timer refresh data
            _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += RefreshData;
                _timer.Enabled = true;
                #endregion
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async void RefreshData(object? sender, ElapsedEventArgs e)
        {
            //try
            {
                await AddDataAsync();

                _timerTick = false;

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
            }
            ///catch { _timerTick = false; }
        }

        private async Task AddDataAsync()
        {
            //try
            {
                if (!_timerTick)
                {
                    _timerTick = true;

                    //data3.AddIf(true, () => new { year = $"{DateTime.Now.ToString("HH:mm:ss")}", Type = "S1", value = 3 });
                    //data3.AddIf(true, () => new { year = $"{DateTime.Now.ToString("HH:mm:ss")}", Type = "S2", value = 7 });

                    var d=data3.Count();

                    data3.Remove(0);
                    data3.Remove(1);

                    var d1 = data3.Count();

                   
                }
            }
            //catch { }
        }

        #region 示例3

        object[] data3 =
                        {
                            new {year = $"{DateTime.Now.ToString("HH:mm:ss")}", Type="S1", value = 3},
                            new {year =  $"{DateTime.Now.ToString("HH:mm:ss")}", Type="S2", value = 5},
                            new {year = $"{DateTime.Now.AddSeconds(10).ToString("HH:mm:ss")}", Type="S1", value = 7},
                            new {year =  $"{DateTime.Now.AddSeconds(10).ToString("HH:mm:ss")}", Type="S2", value = 9},
                            new {year = $"{DateTime.Now.AddSeconds(20).ToString("HH:mm:ss")}", Type="S1", value = 5},
                            new {year =  $"{DateTime.Now.AddSeconds(20).ToString("HH:mm:ss")}", Type="S2", value = 10},
                        };

        [Obsolete]
        LineConfig config3 = new LineConfig
        {
            Title = new Title
            {
                Visible = true,
                Text = "test"
            },
            Description = new Description
            {
                Visible = true,
                Text = "Mo ta"
            },
            ForceFit = true,
            Padding = "auto",
            XField = "year",
            YField = "value",
            //Smooth = true,
            //YAxis = new ValueAxis
            //{
            //    Label = new BaseAxisLabel()
            //},
            Legend = new Legend
            {
                Position = "right-top"
            },

            Point = new LineViewConfigPoint
            {
                Visible = true,
                Size = 5,
                Shape = "diamond",
                Style = new GraphicStyle
                {
                    Fill = "white",
                    Stroke = "#2593fc",
                    LineWidth = 2
                }
            },
            Label = new AntDesign.Charts.Label
            {
                Visible = true,
                Type = "point"
            },
            SeriesField = "type",
            Color = new string[] { "#1979C9", "#D62A0D" },
            Responsive = true,
        };
        #endregion 示例3
    }
}
