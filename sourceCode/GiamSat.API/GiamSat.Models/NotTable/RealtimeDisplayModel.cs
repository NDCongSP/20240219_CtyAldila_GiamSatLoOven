using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Chứa thông tin cần hiển thị của 1 lò.
    /// </summary>
    public class RealtimeDisplayModel
    {
        public int OvenId { get; set; }
        public string OvenName { get; set; }
        /// <summary>
        /// Lưu thông tin tag path, để phục vụ cho sự kiện tagCHanged.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Trạng thái lò Chạy/Dừng.
        /// 0-Dừng; 1- Chạy.
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// trạng thái kết nối modbus đến lò.
        /// 1-Kết nối; 0 - mất kết nối.
        /// </summary>
        public int ConnectionStatus { get; set; }
        /// <summary>
        /// Cảnh báo. 1 là cảnh báo, 0 là bình thường.
        /// </summary>
        public int Alarm { get; set; }
        /// <summary>
        /// dieennx giải alarm.
        /// </summary>
        public string AlarmDescription { get; set; }
        public double Temperature { get; set; } = 0;
        /// <summary>
        /// Trạng thái cửa.
        /// 0-Mo; 1- dong
        /// </summary>
        public int DoorStatus { get; set; }
        /// <summary>
        /// Profile đang chạy.
        /// </summary>
        public int ProfileNumber_CurrentStatus { get; set; }
        public string ProfileName { get; set; }
        public EnumProfileStepType StepName { get; set; }
        /// <summary>
        /// Bước đang chạy.
        /// </summary>
        public int ProfileStepNumber_CurrentStatus { get; set; }
        public EnumProfileStepType LastStepType { get; set; }
        /// <summary>
        /// Kiểu bước chạy.
        /// RampTime,RampRate,Soak,Kump,End
        /// </summary>
        public EnumProfileStepType ProfileStepType_CurrentStatus { get; set; }
        public int HoursRemaining_CurrentStatus { get; set; }
        public int MinutesRemaining_CurrentStatus { get; set; }
        public int SecondsRemaining_CurrentStatus { get; set; }
        /// <summary>
        /// dùng để lưu lại data giây chạy phục vị cho việc xác định máy chạy hay là máy dừng.
        /// </summary>
        public int SecondsRemaining_CurrentStatusOld { get; set; }
        //đếm số lần second mới và second cũ giống nhau, để đưa ra tín hiệu lò dừng chạy.
        public int CountSecondStop { get; set; }

        /// <summary>
        /// Chốt thời gian bắt đầu chạy profile để tính thời gian chạy, so sánh với thời gian cài đặt nếu không đạt nhiệt độ để cảnh báo.
        /// </summary>
        public string BeginTime { get; set; }

        #region Canh bao cua  bước
        /// <summary>
        /// Thời gian chạy (h).
        /// </summary>
        public int Hours { get; set; }
        /// <summary>
        /// Thời gian chạy (m).
        /// </summary>
        public int Minutes { get; set; }
        /// <summary>
        /// Thời gian chạy (s).
        /// </summary>
        public int Seconds { get; set; }
        /// <summary>
        /// Nhiệt độ cao nhất của bước chạy trước, dùng để so sánh với setPoint của step kế tiếp, để biết nhiệt độ đang muốn điều khiển tăng hay giảm để so sánh cảnh báo cho đúng.
        /// </summary>
        /// 
        public double SetPointLastStep { get; set; }
        /// <summary>
        /// Ngưỡng cao nhiệt độ. Dùng để cảnh báo.
        /// </summary>
        public double SetPoint { get; set; }
        /// <summary>
        /// Khoảng nhiệt độ của bước chạy cần tăng.
        /// </summary>
        public double TempRange { get; set; }
        /// <summary>
        /// Tổng thời gian chạy của bước thính theo phút.
        /// </summary>
        public double TotalTimeRunMinute
        {
            set { }
            get
            {
                var tt = TimeSpan.FromHours(Hours).TotalMinutes + TimeSpan.FromMinutes(Minutes).TotalMinutes + TimeSpan.FromSeconds(Seconds).TotalMinutes;
                return Math.Round(tt, 2);
            }
        }
        /// <summary>
        /// Thời gian cần thay đổi theo từng phút.
        ///  TempRange / TotalTimeRunMinute.
        ///  Dung cho việc cảnh báo.
        /// </summary>
        public double TempRateMinute
        {
            set { }
            get { return Math.Round(TempRange / TotalTimeRunMinute, 2); }
        }

        /// <summary>
        /// Được bật lên khi hết thời gian, để cảnh báo vào set xem nhiệt độ có đạt ko để cảnh báo, rồi mới cho về 0.
        /// </summary>
        public int EndStep { get; set; } = 0;
        #endregion

        /// <summary>
        /// Số thứ tự mỗi lần chạy.được tạo ra mỗi khi bắt đầu run profile.Dùng cho việc lưu dataLog khi run Profile.
        /// được tạo trong sự kiện tag value changed ProfileNumber_CurrentStatus_ValueChanged
        /// </summary>
        public Guid ZIndex { get; set; }
        public OvenInfoModel OvenInfo { get; set; }

        public bool StatusFlag { get; set; } = false;
        public bool AlarmFlag { get; set; } = false;
        /// <summary>
        /// dùng để reset biến AlarmFlag ở bước Soak khi mới khởi động chương trình, mà nhiệt độ ko vượt ngưỡng.
        /// thì vào reset lai biến AlarmFlag để nó reset alame.
        /// False-cho vao reset; True-ko cho vao reset.
        /// </summary>
        public bool ResetAlarmFlag { get; set; } = false;
        public bool AlarmFlagLastStep { get; set; } = false;
        public bool IsLoaded { get; set; } = false;

        //dung cho việc xác định máy chạy hay dừng, dựa vào sự kiện tag value changed của tag second.
        public DateTime StatusTimeBegin { get; set; }
        public DateTime StatusTimeEnd { get; set; }
        /// <summary>
        /// =2 thi moi cho lưu data profile.
        /// </summary>
        public int CountSecondTagChange { get; set; } = 0;

        /// <summary>
        /// Báo còi đang bật hay tắt.
        /// </summary>
        public int SerienStatus { get; set; }

        /// <summary>
        /// Biến dùng để check alarm trong suốt thời gian chạy profile.
        /// </summary>
        public DateTime BeginTimeAlarm { get; set; }
        /// <summary>
        /// Biến dùng để check alarm trong suốt thời gian chạy profile.
        /// </summary>
        public DateTime EndTimeAlarm { get; set; }
        public double LevelUp { get; set; } = 0;
        public double LevelDown { get; set; } = 0;

        /// <summary>
        /// Biến dùng để lưu thời gian bắt đầu của bước.
        /// Phục vụ cho việc xét alarm thời gian thực trong qua trình chạy profile.
        /// </summary>
        public DateTime BeginTimeOfStep { get; set; }

        /// <summary>
        /// Nhiệt độ cần đặt được tới thời điểm hiện tại.
        /// DÙng cho cảnh báo toàn thời gian khi chạy profile.
        /// </summary>
        public double TempRequired { get; set; } = 0;

        /// <summary>
        /// Nhiệt độ ban đầu khi bắt đầu step.
        /// Dùng cho cảnh báo toàn thời gian khi chạy profile.
        /// </summary>
        public double TempBeginStep { get; set; } = 0;

        /// <summary>
        /// Dùng để bật cho phép so sánh nhiệt độ để bật cảnh báo khi chạy profile.
        /// </summary>
        public bool IsCheckAlarm { get; set; } = false;
    }
}
