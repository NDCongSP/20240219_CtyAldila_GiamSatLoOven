using System;

namespace GiamSat.Models
{
    public class SandingRealtimeModel
    {
        public int SandingId { get; set; } = 1;
        public string SandingName { get; set; } = "Auto Sanding";
        public string? Path { get; set; } = "Local Station/Channel1/Device1";

        public bool PlcConnected { get; set; }
        public bool EasyDriverConnected { get; set; }
        public DateTime LastUpdated { get; set; }

        public string PartName { get; set; } = string.Empty;
        public string Work { get; set; } = string.Empty;
        public int LogStyle { get; set; }
        public string SandingMode { get; set; } = "Production"; // "Production" (1) or "Test" (2)

        // PLC Live Status Tags
        public int Set_Trigger_NewPartInfo { get; set; }
        public int Trigger_Log_Sanding { get; set; }
        public int Trigger_Log_OD { get; set; }
        public int Shaft_Num_Sanding { get; set; }
        public double Spine_A { get; set; }
        public double Mortor_Sanding_Speed { get; set; }
        public double Spine_B { get; set; }
        public double Spine_Target { get; set; }
        public double Spine_Low { get; set; }
        public double Spine_Hight { get; set; }
        public int OK_NG_Sanding { get; set; }
        public int Shaft_Num_OD { get; set; }
        public double Diam_Reading_1 { get; set; }
        public double Diam_Reading_2 { get; set; }
        public double Diam_Reading_3 { get; set; }
        public int OK_NG_OD_1 { get; set; }
        public int OK_NG_OD_2 { get; set; }
        public int OK_NG_OD_3 { get; set; }

        // PLC Downloaded Config Tags (Set_)
        public int Set_Shaft_Length { get; set; }
        public int Set_Freq_Target { get; set; }
        public int Set_Freq_Offset_Low { get; set; }
        public int Set_Freq_Offset_Hight { get; set; }
        public int Set_Formula_F { get; set; }
        public int Set_A { get; set; }
        public int Set_B { get; set; }
        public int Set_C { get; set; }
        public int Set_D { get; set; }
        public int Set_Diam_LL_1 { get; set; }
        public int Set_Diam_LL_2 { get; set; }
        public int Set_Diam_LL_3 { get; set; }
        public int Set_Diam_UL_1 { get; set; }
        public int Set_Diam_UL_2 { get; set; }
        public int Set_Diam_UL_3 { get; set; }
        public double Set_Tip_OD_Length_1 { get; set; }
        public double Set_Tip_OD_Length_2 { get; set; }
        public double Set_Tip_OD_Length_3 { get; set; }
        public int Auto_Manual { get; set; }
    }
}
