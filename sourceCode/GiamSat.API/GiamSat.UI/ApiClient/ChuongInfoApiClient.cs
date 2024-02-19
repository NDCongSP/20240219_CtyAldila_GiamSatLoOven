using GiamSat.Models;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GiamSat.UI
{
    public class ChuongInfoApiClient : ISChuongInfo
    {
        readonly HttpClient _httpClient;

        public ChuongInfoApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<ChuongInfoModel>>> GetAll()
        {
            try
            {
                var res = await _httpClient.GetFromJsonAsync<Result<List<ChuongInfoModel>>>($"{ApiRoutes.ChuongInfo.BasePath}");
                return res;
            }
            catch (Exception ex)
            {
                return await Result<List<ChuongInfoModel>>.FailAsync(ex.Message);
            }
        }

        public Task<Result<ChuongInfoModel>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ChuongInfoModel>> GetByName([Path] string id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ChuongInfoModel>> Insert([Body] ChuongInfoModel model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ChuongInfoModel>> Update([Body] ChuongInfoModel model)
        {
            throw new NotImplementedException();
        }
    }
}
