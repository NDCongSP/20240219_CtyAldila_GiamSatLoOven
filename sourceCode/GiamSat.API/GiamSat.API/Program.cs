using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.IO;

namespace GiamSat.API
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Console.WriteLine làm fallback — đảm bảo bạn luôn thấy output kể cả Serilog có lỗi.
            Console.WriteLine("=== GiamSat.API booting ===");

            var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDir);

            // Bootstrap logger — đơn giản, chỉ Console.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Console.WriteLine("=== Building host... ===");
                Log.Information("Starting GiamSat.API host");
                var host = CreateHostBuilder(args).Build();
                Console.WriteLine("=== Host built. Running... ===");
                host.Run();
                Console.WriteLine("=== Host stopped. ===");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== FATAL: {ex} ===");
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, lc) =>
                {
                    var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");

                    // Console: format ngắn cho dễ đọc.
                    var consoleTemplate =
                        "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}";

                    // File: format đầy đủ ngày giờ + timezone — PHẢI khớp regex parse trong LogsController.
                    var fileTemplate =
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}";

                    lc.MinimumLevel.Information()
                      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                      .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                      .MinimumLevel.Override("System", LogEventLevel.Warning)
                      .Enrich.FromLogContext()
                      .WriteTo.Console(outputTemplate: consoleTemplate)
                      .WriteTo.File(
                          path: Path.Combine(logDir, "all-.log"),
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: 31,
                          shared: true,
                          outputTemplate: fileTemplate)
                      .WriteTo.File(
                          path: Path.Combine(logDir, "error-.log"),
                          restrictedToMinimumLevel: LogEventLevel.Error,
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: 90,
                          shared: true,
                          outputTemplate: fileTemplate)
                      .WriteTo.File(
                          formatter: new CompactJsonFormatter(),
                          path: Path.Combine(logDir, "structured-.json"),
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: 31,
                          shared: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
