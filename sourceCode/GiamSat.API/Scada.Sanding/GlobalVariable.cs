using GiamSat.Models;
using System;
using System.Windows.Forms;

namespace Scada.Sanding
{
    public static class GlobalVariable
    {
        public static string ConnectionString { get; set; } = string.Empty;
        public static string? IpDbServer { get; set; }

        public static SandingRealtimeModel SandingRealtime { get; set; } = new SandingRealtimeModel();

        public static int Pilot5SandingCount { get; set; } = 0;
        public static int Pilot5OdCount { get; set; } = 0;

        public static void InvokeIfRequired(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
