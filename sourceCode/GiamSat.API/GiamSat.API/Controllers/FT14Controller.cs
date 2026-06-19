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
    public class FT14Controller : BaseController<Guid, FT14_TipOdFreq>, ISFT14
    {
        readonly SCommon _sCommon;
        readonly ISFT14_CalcData _calcDataService;
        readonly ISFT14_Sync _syncService;

        public FT14Controller(SCommon sCommon = null, ISFT14_CalcData calcDataService = null, ISFT14_Sync syncService = null) : base(sCommon.SFT14)
        {
            _sCommon = sCommon;
            _calcDataService = calcDataService;
            _syncService = syncService;
        }

        /// <summary>
        /// Lấy dữ liệu Fre1/Fre2 từ external DB để tính A,B,C,D.
        /// </summary>
        [HttpGet("calcdata")]
        public async Task<Result<List<AutoSandingTestRow>>> GetCalcData(
            [FromQuery] string part,
            [FromQuery] string workFre1,
            [FromQuery] string workFre2  = "",
            [FromQuery] string workSpine = "",
            [FromQuery] double offsetFre1 = 0,
            [FromQuery] double offsetFre2 = 0,
            [FromQuery] double motorFrom  = 100,
            [FromQuery] double motorTo    = 500,
            [FromQuery] double motorStep  = 100)
        {
            return await _calcDataService.GetCalcDataAsync(part, workFre1, workFre2, workSpine, offsetFre1, offsetFre2, motorFrom, motorTo, motorStep);
        }

        /// <summary>
        /// Lấy danh sách Work Order liên quan tới một Part (cho dropdown Fre1/Fre2/Spine).
        /// </summary>
        [HttpGet("works")]
        public async Task<Result<PartWorksDto>> GetWorks([FromQuery] string part)
        {
            return await _calcDataService.GetWorksAsync(part);
        }

        /// <summary>
        /// Lấy danh sách Part nguồn (PartId + PartName) từ external DB ALD_MFG để chia batch đồng bộ.
        /// </summary>
        [HttpGet("sync/sources")]
        public async Task<Result<List<PartSyncSourceDto>>> GetSyncSources()
        {
            return await _syncService.GetSyncSourcesAsync();
        }

        /// <summary>
        /// Đồng bộ một batch Part (theo PartId) từ external DB vào bảng FT14.
        /// </summary>
        [HttpPost("sync")]
        public async Task<Result<FT14SyncResultDto>> SyncParts([FromBody] List<int> partIds)
        {
            return await _syncService.SyncPartsAsync(partIds);
        }

        /// <summary>
        /// Xóa cứng một Part khỏi database theo Id (không soft-delete Actived=0).
        /// </summary>
        [HttpDelete(ApiRoutes.Delete)]
        public Task<Result<bool>> Delete([FromRoute] Guid id)
        {
            return _sCommon.SFT14.Delete(id);
        }
    }
}
