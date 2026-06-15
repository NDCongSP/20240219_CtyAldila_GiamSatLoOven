namespace GiamSat.APIClient
{
    public partial class RevoReportStepVmListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
    }

    public partial class RevoReportShaftVmListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
    }

    public partial class RevoReportHourVmListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
    }

    public partial class RevoFilterModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("skip")]
        public int Skip { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("take")]
        public int Take { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sortColumn")]
        public string? SortColumn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sortDescending")]
        public bool SortDescending { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isExport")]
        public bool IsExport { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("connectionId")]
        public string? ConnectionId { get; set; }
    }
}
