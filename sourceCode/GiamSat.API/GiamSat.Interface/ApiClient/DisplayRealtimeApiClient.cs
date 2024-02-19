using GiamSat.Models;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GiamSat.Interface
{
    public class DisplayRealtimeApiClient : ISDisplayRealtime
    {
        readonly HttpClient _httpClient;

        public DisplayRealtimeApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<DisplayRealTimeModel>>> GetAll()
        {
            try
            {
                var res = await _httpClient.GetFromJsonAsync<Result<List<DisplayRealTimeModel>>>($"{ApiRoutes.DisplayRealtime.BasePath}");
                return res;
            }
            catch (Exception ex)
            {
                return await Result<List<DisplayRealTimeModel>>.FailAsync(ex.Message);
            }
        }

        public Task<Result<DisplayRealTimeModel>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<DisplayRealTimeModel>>> GetTop2()
        {
            throw new NotImplementedException();
        }

        public Task<Result<DisplayRealTimeModel>> Insert([Body] DisplayRealTimeModel model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<DisplayRealTimeModel>> Update([Body] DisplayRealTimeModel model)
        {
            throw new NotImplementedException();
        }
    }
}
