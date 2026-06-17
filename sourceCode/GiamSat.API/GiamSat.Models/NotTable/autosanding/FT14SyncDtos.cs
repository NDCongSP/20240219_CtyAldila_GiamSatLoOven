using System.Collections.Generic;

namespace GiamSat.Models
{
    /// <summary>
    /// Part nguồn để đồng bộ — đọc từ external DB ALD_MFG (bảng Part).
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.Models/NotTable/autosanding/FT14SyncDtos.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// </remarks>
    public class PartSyncSourceDto
    {
        public int PartId { get; set; }
        public string? PartName { get; set; }
    }

    /// <summary>
    /// Kết quả đồng bộ một batch Part vào bảng Oven.FT14.
    /// </summary>
    public class FT14SyncResultDto
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Danh sách Work Order liên quan tới một Part (dùng cho dropdown Tab Tính A,B,C,D).
    /// FreWorks  = từ external DB (DatalogFrequency) — dùng cho Fre1/Fre2.
    /// SpineWorks = từ FT16 (SandingMode=Test) — dùng cho Stiffness/Spine.
    /// </summary>
    public class PartWorksDto
    {
        public List<string> FreWorks { get; set; } = new List<string>();
        public List<string> SpineWorks { get; set; } = new List<string>();
    }
}
