using System.Collections.Generic;

namespace GiamSat.Models
{
    public class ProfileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Dùng cho việc vẽ thêm giớ hạn cao nhất trong khi chạy profile UI.
        /// </summary>
        public double LevelUp { get; set; }
        /// <summary>
        /// Dùng cho việc vẽ thêm giới hạn thấp nhất trong khi chạy profile UI.
        /// </summary>
        public double LevelDown { get; set; }
        public List<StepModel> Steps { get; set; }
    }
}