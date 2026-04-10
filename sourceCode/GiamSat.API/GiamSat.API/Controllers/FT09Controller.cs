using GiamSat.API.Services;
using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public Task<Result<List<RevoReportStepVm>>> GetReportStepView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportStepView(model);
        }

        [HttpPost("GetReportShaftView")]
        public Task<Result<List<RevoReportShaftVm>>> GetReportShaftView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportShaftView(model);
        }

        [HttpPost("GetReportHourView")]
        public Task<Result<List<RevoReportHourVm>>> GetReportHourView([FromBody] RevoFilterModel model)
        {
            return _sCommon.SFT09.GetReportHourView(model);
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
