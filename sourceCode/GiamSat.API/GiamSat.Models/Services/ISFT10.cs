using RestEase;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT10.BasePath)]
    public interface ISFT10
    {
        [Get]
        Task<Result<TemperatureConfigsModel>> GetConfigs();

        [Post]
        Task<Result<bool>> SaveConfigs([Body] TemperatureConfigsModel config);
    }
}
