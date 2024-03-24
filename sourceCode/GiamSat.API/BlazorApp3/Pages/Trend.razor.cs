using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Timers;

namespace BlazorApp3.Pages
{
    public partial class Trend
    {
        bool smooth = false;
        bool showDataLabels = false;
        bool showMarkers = true;

        private System.Timers.Timer _timer = new System.Timers.Timer();

        Radzen.Blazor.RadzenChart RadzenChart = new Radzen.Blazor.RadzenChart();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await RadzenChart.Resize(width:100, height: 200);
                _timer.Interval = 1000;
                _timer.Elapsed += Refresh;

                _timer.Enabled = true;
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void Refresh(object? sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;

                Random rnd = new Random();
                string date = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss");

                for (int j = 0; j < 4; j++)
                {
                    Console.WriteLine(rnd.Next());
                }

                if (revenue2019.Count >= 30)
                {
                    revenue2019.RemoveAt(0);
                    revenue2020.RemoveAt(0);
                }

                revenue2019.Add(new DataItem()
                {
                    Date = date,
                    Revenue = rnd.NextDouble(),
                });

                revenue2020.Add(new DataItem()
                {
                    Date = date,
                    Revenue = rnd.NextDouble(),
                });

                await RadzenChart.Reload();

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT

                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                _timer.Enabled = true;
            }
        }

        class DataItem
        {
            public string Date { get; set; }
            public double Revenue { get; set; }
        }

        string FormatAsUSD(object value)
        {
            return ((double)value).ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
        }

        string FormatAsMonth(object value)
        {
            if (value != null)
            {
                return Convert.ToDateTime(value).ToString("HH:mm:ss");
            }

            return string.Empty;
        }

        List<DataItem> revenue2019 = new List<DataItem>() {
            new DataItem
            {
                Date = ("2024-01-15 09:37:00"),
                Revenue = 234000
            },
            new DataItem
            {
                Date = ("2024-01-15 09:37:10"),
                Revenue = 269000
            },
        };
        List<DataItem> revenue2020 = new List<DataItem>()
        {
            new DataItem
            {
                Date = ("2024-01-15 09:37:00"),
                Revenue = 254000
            },
            new DataItem
            {
                Date = ("2024-01-15 09:37:10"),
                Revenue = 299000
            },
        };

        //    List<DataItem> revenue2019 = new List<DataItem> {
        //    new DataItem
        //    {
        //        Date = ("2019-01-01 "),
        //        Revenue = 234000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-02-01"),
        //        Revenue = 269000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-03-01"),
        //        Revenue = 233000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-04-01"),
        //        Revenue = 244000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-05-01"),
        //        Revenue = 214000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-06-01"),
        //        Revenue = 253000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-07-01"),
        //        Revenue = 274000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-08-01"),
        //        Revenue = 284000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-09-01"),
        //        Revenue = 273000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-10-01"),
        //        Revenue = 282000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-11-01"),
        //        Revenue = 289000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-12-01"),
        //        Revenue = 294000
        //    }
        //};

        //    List<DataItem> revenue2020 = new List<DataItem> {
        //    new DataItem
        //    {
        //        Date = ("2019-01-01"),
        //        Revenue = 334000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-02-01"),
        //        Revenue = 369000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-03-01"),
        //        Revenue = 333000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-04-01"),
        //        Revenue = 344000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-05-01"),
        //        Revenue = 314000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-06-01"),
        //        Revenue = 353000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-07-01"),
        //        Revenue = 374000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-08-01"),
        //        Revenue = 384000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-09-01"),
        //        Revenue = 373000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-10-01"),
        //        Revenue = 382000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-11-01"),
        //        Revenue = 389000
        //    },
        //    new DataItem
        //    {
        //        Date = ("2019-12-01"),
        //        Revenue = 394000
        //    }
        //};
    }
}
