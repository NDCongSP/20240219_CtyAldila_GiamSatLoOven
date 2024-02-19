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
    public class DataLogApiClient : ISDataLog
    {
        readonly HttpClient _httpClient;

        public DataLogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<DataLogModel>>> GetAll()
        {
            try
            {
                var res = await _httpClient.GetFromJsonAsync<Result<List<DataLogModel>>>($"{ApiRoutes.DataLog.BasePath}");
                return res;
            }
            catch (Exception ex)
            {
                return await Result<List<DataLogModel>>.FailAsync(ex.Message);
            }
        }

        public Task<Result<DataLogModel>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<DataLogModel>> Insert([Body] DataLogModel model)
        {
            throw new NotImplementedException();
        }

        public Task<Result> TestApi()
        {
            throw new NotImplementedException();
        }

        public Task<Result<DataLogModel>> Update([Body] DataLogModel model)
        {
            throw new NotImplementedException();
        }
    }
}
