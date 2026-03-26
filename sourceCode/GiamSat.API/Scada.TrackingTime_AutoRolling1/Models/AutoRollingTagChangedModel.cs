using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.TrackingTime_AutoRolling1
{
    public class AutoRollingTagChangedModel
    {
        public int RevoId { get; set; } = 0;

        public string Path { get; set; } = string.Empty;

        public bool PlcConnected { get; set; } = false;

        public string Part_Name { get; set; } = string.Empty;

       

        //---------------------------------------------------------------------------------
        //các biến dưới này là sẽ đọc giá trị từ các tag lên và lưu vao database, để phục vụ cho việc truy xuất thông tin cấu hình theo part, và hiển thị thông tin part đang chạy trên giao diện.
        public string Part_Code { get; set; } = string.Empty;
        public int Recipe_Settings { get; set; } = 0;
        public int StepRun { get; set; } = 0;

        public double TimeRunStep1 { get; set; } = 0;
        public double TimeRunStep2 { get; set; } = 0;
        public double TimeRunStep3 { get; set; } = 0;
        public double TimeRunStep4 { get; set; } = 0;
        public double TimeRunStep5 { get; set; } = 0;
        public double TimeRunStep6 { get; set; } = 0;
        public double TimeRunStep7 { get; set; } = 0;
        public double TimeRunStep8 { get; set; } = 0;
        public double TimeRunStep9 { get; set; } = 0;
        public double TimeRunStep10 { get; set; } = 0;
        public double TimeRunStep11 { get; set; } = 0;
        public double TimeRunStep12 { get; set; } = 0;
        public double TimeRunStep13 { get; set; } = 0;
        public double TimeRunStep14 { get; set; } = 0;
        public double TimeRunStep15 { get; set; } = 0;

        public int Part_Name1 { get; set; } = 0;
        public int Part_Name2 { get; set; } = 0;
        public int Part_Name3 { get; set; } = 0;
        public int Part_Name4 { get; set; } = 0;
        public int Part_Name5 { get; set; } = 0;
        public int Part_Name6 { get; set; } = 0;
        public int Part_Name7 { get; set; } = 0;
        public int Part_Name8 { get; set; } = 0;
    }
}
