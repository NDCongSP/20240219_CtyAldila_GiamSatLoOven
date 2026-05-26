using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT11.BasePath)]
    public interface ISFT11
    {
        [Get(ApiRoutes.FT11.GetRealtime)]
        Task<Result<List<TemperatureRealtimeModel>>> GetRealtime();

        [Get(ApiRoutes.FT11.GetAlarmLogs)]
        Task<Result<List<FT13_TemperatureAlarmLog>>> GetAlarmLogs([Query] DateTime fromDate, [Query] DateTime toDate);

        [Get(ApiRoutes.FT11.GetDataLogs)]
        Task<Result<List<FT12_TemperatureDatalog>>> GetDataLogs([Query] DateTime fromDate, [Query] DateTime toDate);

        [Post(ApiRoutes.FT11.SyncRealtime)]
        Task<Result<bool>> SyncRealtime([Body] List<TemperatureRealtimeModel> reqData);

        [Post(ApiRoutes.FT11.SyncDatalog)]
        Task<Result<bool>> SyncDatalog([Body] List<TemperatureRealtimeModel> reqData);
    }
}
