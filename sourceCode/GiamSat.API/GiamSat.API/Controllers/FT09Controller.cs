using GiamSat.API.Services;
using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GiamSat.API.Services.ExportWorker;

namespace GiamSat.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FT09Controller : BaseController<Guid, FT09_RevoDatalog>, ISFT09
    {
        readonly SCommon _sCommon;

        public FT09Controller(SCommon sCommon = null) : base(sCommon?.SFT09)
        {
            _sCommon = sCommon;
        }

        [HttpGet("GetTotalShaft")]
        public Task<Result<List<RevoGetTotalShaftCountDto>>> GetTotalShaft([FromQuery] int? revoId = null)
        {
            return _sCommon.SFT09.GetTotalShaft(revoId);
        }

        [HttpPost("GetFilter")]
        public Task<Result<List<FT09_RevoDatalog>>> GetFilter([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetFilter(model);
        }

        [HttpPost("GetReportStepView")]
        public Task<PagedResult<RevoReportStepVm>> GetReportStepView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportStepView(model);
        }

        [HttpPost("GetReportShaftView")]
        public Task<PagedResult<RevoReportShaftVm>> GetReportShaftView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportShaftView(model);
        }

        [HttpPost("GetReportHourView")]
        public Task<PagedResult<RevoReportHourVm>> GetReportHourView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportHourView(model);
        }

        [HttpPost("ExportExcelAsync")]
        public IActionResult ExportExcelAsync(
            [FromBody] RevoFilterModel model,
            [FromServices] Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory,
            [FromServices] Microsoft.AspNetCore.SignalR.IHubContext<GiamSat.API.Hubs.ReportHub> hubContext,
            [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(model.ConnectionId))
            {
                return BadRequest("ConnectionId is required for async export.");
            }

            Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var sCommon = scope.ServiceProvider.GetRequiredService<GiamSat.API.SCommon>();

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 30);

                    model.Take = -1; // Lấy tất cả
                    model.Skip = 0;
                    
                    IReadOnlyList<Services.ExportWorker.RevoStepRow>? stepRows = null;
                    IReadOnlyList<Services.ExportWorker.RevoShaftRow>? shaftRows = null;
                    IReadOnlyList<Services.ExportWorker.RevoHourRow>? hourRows = null;
                    
                    var mode = (Services.ExportWorker.RevoReportMode)model.ExportMode;
                    long totalRecords = 0;
                    int shaftTotal = 0;
                    int shaftFinished = 0;

                    if (mode == Services.ExportWorker.RevoReportMode.ByStep)
                    {
                        var res = await sCommon.SFT09.GetReportStepView(model);
                        if (res != null && res.Succeeded)
                        {
                            shaftTotal = res.Data?.Select(x => x.ShaftKey).Distinct().Count() ?? 0;
                            shaftFinished = res.Data?.Where(x => x.IsShaftFinished == true).Select(x => x.ShaftKey).Distinct().Count() ?? 0;

                            stepRows = res.Data?.Select(x => new Services.ExportWorker.RevoStepRow
                            {
                                Stt = (int)x.Stt,
                                RevoName = x.RevoName,
                                ShaftKey = x.ShaftKey ?? "",
                                Part = x.Part,
                                Rev = x.Rev,
                                Mandrel = x.Mandrel,
                                StepId = x.StepId,
                                StepDisplay = x.StepDisplay ?? "",
                                StartedAt = x.DisplayStartedAt,
                                EndedAt = x.DisplayEndedAt,
                                DurationText = x.DurationText ?? "",
                                HighlightIncomplete = x.HighlightIncomplete,
                                Work = x.Work
                            }).ToList();
                            totalRecords = res.TotalRecords;
                        }
                    }
                    else if (mode == Services.ExportWorker.RevoReportMode.ByShaft)
                    {
                        var res = await sCommon.SFT09.GetReportShaftView(model);
                        if (res != null && res.Succeeded)
                        {
                            shaftTotal = res.Data?.Count ?? 0;
                            shaftFinished = res.Data?.Count(x => x.IsShaftFinished) ?? 0;

                            shaftRows = res.Data?.Select(x => new Services.ExportWorker.RevoShaftRow
                            {
                                Stt = (int)x.Stt,
                                ShaftLabel = x.ShaftLabel ?? "",
                                Part = x.Part,
                                Mandrel = x.Mandrel,
                                StepCount = (int)x.StepCount,
                                StartedAt = x.StartedAt,
                                EndedAt = x.EndedAt,
                                TotalTimeText = x.TotalTimeText ?? "",
                                HighlightIncomplete = x.HighlightIncomplete,
                                Work = x.Work
                            }).ToList();
                            totalRecords = res.TotalRecords;
                        }
                    }
                    else if (mode == Services.ExportWorker.RevoReportMode.ByHour)
                    {
                        var res = await sCommon.SFT09.GetReportHourView(model);
                        if (res != null && res.Succeeded)
                        {
                            shaftTotal = res.Data?.Sum(x => x.ShaftCount) ?? 0;
                            shaftFinished = (int)(res.Data?.Sum(x => x.ShaftCountFinishedInHour) ?? 0);

                            hourRows = res.Data?.Select(x => new Services.ExportWorker.RevoHourRow
                            {
                                HourRange = x.Hour.ToString("yyyy-MM-dd HH") + ":00-" + x.Hour.AddHours(1).ToString("HH") + ":00",
                                HighlightIncomplete = x.HighlightIncomplete
                            }).ToList();
                            totalRecords = res.TotalRecords;
                        }
                    }

                    if (totalRecords == 0)
                    {
                        await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportFailed", "Khong co du lieu de xuat.");
                        return;
                    }

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 50);

                    // Fetch Revo list
                    var revoRes = await sCommon.SFT07.GetAll();
                    var allRevoList = new List<Services.ExportWorker.RevoDropdownModel>();
                    if (revoRes.Succeeded && revoRes.Data != null)
                    {
                        var ft07 = revoRes.Data.FirstOrDefault(x => x.Actived == true) ?? revoRes.Data.FirstOrDefault();
                        if (ft07 != null && !string.IsNullOrEmpty(ft07.C000))
                        {
                            try
                            {
                                var revoConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(ft07.C000);
                                if (revoConfigs != null)
                                {
                                    foreach (var c in revoConfigs)
                                    {
                                        allRevoList.Add(new Services.ExportWorker.RevoDropdownModel { Id = (int)c.Id, Name = (string)c.Name });
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    var excelExport = new Services.ExportWorker.ExcelExportRevo();

                    var dateQuery = $"{model.FromDate:dd/MM/yyyy HH:mm:ss} den {model.ToDate:dd/MM/yyyy HH:mm:ss}";
                    var shaftKind = (model.ShaftScope == "finished") ? "Chỉ shaft hoàn thành" : "Tất cả shaft";
                    
                    var shaftLine = (model.ShaftScope == "finished")
                        ? $"Phạm vi: {shaftKind}  |  Shaft (theo phạm vi): {shaftFinished}  |  Tổng số bản ghi: {totalRecords}"
                        : $"Phạm vi: {shaftKind}  |  Tổng số Shaft: {shaftTotal}  |  Tổng số bản ghi: {totalRecords}";

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 70);

                    var excelBytes = excelExport.GenerateExcelFileAsync(
                        dateQuery,
                        shaftLine,
                        mode,
                        stepRows,
                        shaftRows,
                        hourRows,
                        allRevoList);

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 90);

                    var filename = $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var reportDir = System.IO.Path.Combine(env.WebRootPath, "reports");
                    if (!System.IO.Directory.Exists(reportDir))
                    {
                        System.IO.Directory.CreateDirectory(reportDir);
                    }
                    var filePath = System.IO.Path.Combine(reportDir, filename);
                    await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);

                    var fileUrl = $"/reports/{filename}";
                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportCompleted", fileUrl);
                }
                catch (Exception ex)
                {
                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportFailed", ex.Message);
                }
            });

            return Ok(new { Message = "Export job queued successfully." });
        }

        [HttpPost("ExportPdf")]
        public async Task<IActionResult> ExportPdf([FromBody] RevoFilterModel model)
        {
            try
            {
                model.Take = -1;
                model.Skip = 0;
                var result = await _sCommon.SFT09.GetReportStepView(model);
                
                if (result == null || !result.Succeeded || result.Data == null || result.Data.Count == 0)
                {
                    return BadRequest(new { message = "Không có dữ liệu để xuất PDF" });
                }

                var stepRows = result.Data.Select(x => new Services.ExportWorker.RevoStepRow
                {
                    Stt = (int)x.Stt,
                    RevoName = x.RevoName,
                    ShaftKey = x.ShaftKey ?? "",
                    Part = x.Part,
                    Rev = x.Rev,
                    Mandrel = x.Mandrel,
                    StepId = x.StepId,
                    StepDisplay = x.StepDisplay ?? "",
                    StartedAt = x.DisplayStartedAt,
                    EndedAt = x.DisplayEndedAt,
                    DurationText = x.DurationText ?? "",
                    HighlightIncomplete = x.HighlightIncomplete,
                    Work = x.Work
                }).ToList();

                int totalShafts = result.Data?.Select(x => x.ShaftKey).Distinct().Count() ?? 0;

                var pdfExport = new PdfExportRevo();
                var dateQuery = $"{model.FromDate:dd/MM/yyyy} đến {model.ToDate:dd/MM/yyyy}";
                var pdfBytes = pdfExport.GeneratePdfFile(stepRows, dateQuery, totalShafts);

                var filename = $"BaoCao_REVO_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi xuất PDF: {ex.Message}" });
            }
        }
    }
}
