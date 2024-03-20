namespace GiamSat.Models
{
    public class StepModel
    {
        public int Id { get; set; }
        public EnumProfileStepType StepType { get; set; }
        
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
        /// Set giá trị SV.
        /// </summary>
        public double SetPoint { get; set; }
    }
}