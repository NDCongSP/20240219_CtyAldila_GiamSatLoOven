using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Threading.Tasks;

namespace GiamSat.Interface.Authorization
{
    public static class ExtAccessTokenProvider
    {
        public static async Task<string> GetAccessTokenAsync(this IAccessTokenProvider tokenProvider)
        {
            return (await tokenProvider.RequestAccessToken()).TryGetToken(out var token) ? token.Value : null;
        }
    }
}
