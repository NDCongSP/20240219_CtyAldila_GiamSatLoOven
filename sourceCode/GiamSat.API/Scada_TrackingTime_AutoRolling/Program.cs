using Microsoft.Extensions.Configuration;
using Serilog;

namespace Scada_TrackingTime_AutoRolling
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Directory.CreateDirectory("Logs"); // 🔥 fix chính

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
            path: "Logs\\app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7, // giữ 7 ngày
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
        )
        .CreateLogger();

            Log.Information("Application starting...");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            // Lấy giá trị
            GlobalVariable.ConnectionString = config.GetConnectionString("DefaultConnection");
            //int delay = config.GetValue<int>("AppSettings:DelayTime");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //Application.Run(new Form1());

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}