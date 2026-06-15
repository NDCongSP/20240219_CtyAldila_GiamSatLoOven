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

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 10);

                    var result = await sCommon.SFT09.GetFilter(model);
                    if (result == null || !result.Succeeded || result.Data == null || result.Data.Count == 0)
                    {
                        await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportFailed", "Khong co du lieu de xuat.");
                        return;
                    }

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 50);

                    // Map to dictionary to avoid querying DB again
                    var allRevoList = result.Data
                        .Where(x => x.RevoId > 0 && !string.IsNullOrEmpty(x.RevoName))
                        .GroupBy(x => x.RevoId)
                        .Select(g => new Services.ExportWorker.RevoDropdownModel { Id = g.Key ?? 0, Name = g.First().RevoName })
                        .ToList();

                    var excelExport = new Services.ExportWorker.ExcelExportRevo();

                    var dateQuery = $"{model.FromDate:dd/MM/yyyy HH:mm:ss} den {model.ToDate:dd/MM/yyyy HH:mm:ss}";
                    var shaftKind = (model.ShaftScope == "finished")
                        ? Services.ExportWorker.RevoShaftScopeKind.Finished
                        : Services.ExportWorker.RevoShaftScopeKind.Total;

                    await hubContext.Clients.Client(model.ConnectionId).SendAsync("ExportProgress", 70);

                    var excelBytes = excelExport.GenerateExcelFileAsync(
                        result.Data,
                        dateQuery,
                        (Services.ExportWorker.RevoReportMode)model.ExportMode,
                        shaftKind,
                        false,
                        new List<Services.ExportWorker.RevoStepRow>(),
                        new List<Services.ExportWorker.RevoShaftRow>(),
                        new List<Services.ExportWorker.RevoHourRow>(),
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
                var result = await _sCommon.SFT09.GetFilter(model);
                if (result == null || !result.Succeeded || result.Data == null || result.Data.Count == 0)
                {
                    return BadRequest(new { message = "Không có dữ liệu để xuất PDF" });
                }

                var pdfExport = new PdfExportRevo();
                var dateQuery = $"{model.FromDate:dd/MM/yyyy} đến {model.ToDate:dd/MM/yyyy}";
                var pdfBytes = pdfExport.GeneratePdfFile(result.Data, dateQuery);

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
