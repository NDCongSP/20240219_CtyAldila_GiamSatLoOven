using Serilog;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Scada_TrackingTIme_Revo
{
    internal static class Program
    {
        // Nên dùng GUID để tránh trùng lặp với các phần mềm khác
        private static Mutex mutex = new Mutex(true, "{B7A9E123-SCADA-REVO-TRACKING-TIME}");

        [STAThread]
        static void Main()
        {
            // 1. Cấu hình Culture (Rất tốt cho việc đọc số thực từ PLC)
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 2. Cấu hình Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: "Logs\\app-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                )
                .CreateLogger();

            try
            {
                Log.Information("Ứng dụng bắt đầu khởi động.");

                // 3. Kiểm tra Mutex
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    try
                    {
                        // Thêm xử lý lỗi toàn cục để không bị văng app bất ngờ
                        Application.ThreadException += (s, e) => Log.Error(e.Exception, "Lỗi Thread chính");
                        AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Error((Exception)e.ExceptionObject, "Lỗi hệ thống chưa xác định");

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new frmProduction());
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal(ex, "Ứng dụng bị lỗi nghiêm trọng khi đang chạy.");
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    MessageBox.Show("Ứng dụng đã đang chạy trong hệ thống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Lỗi khởi tạo Mutex hoặc Logging.");
            }
            finally
            {
                // QUAN TRỌNG: Đảm bảo log được đẩy hết xuống file trước khi đóng app
                Log.CloseAndFlush();
            }
        }
    }
}