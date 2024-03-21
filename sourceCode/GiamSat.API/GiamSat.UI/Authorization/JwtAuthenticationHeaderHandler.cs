using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GiamSat.UI    
{
    public class JwtAuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly IAccessTokenProviderAccessor _tokenProviderAccessor;
        private readonly NavigationManager _navigation;

        public JwtAuthenticationHeaderHandler(IAccessTokenProviderAccessor tokenProviderAccessor, NavigationManager navigation)
        {
            _tokenProviderAccessor = tokenProviderAccessor;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // skip token endpoints
            if (request.RequestUri?.AbsolutePath.Contains("/login") is not true)
            {
                var accessTokenResult = await _tokenProviderAccessor.TokenProvider.RequestAccessToken();
                if (accessTokenResult.Status == AccessTokenResultStatus.RequiresRedirect)
                {
                    _navigation.NavigateTo(accessTokenResult.RedirectUrl);
                }
                else if (accessTokenResult.Status == AccessTokenResultStatus.Success)
                {
                    if (accessTokenResult.TryGetToken(out AccessToken accessToken) && !string.IsNullOrWhiteSpace(accessToken.Value))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
                    }
                    else
                    {
                        _navigation.NavigateTo("/login");

                    }
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
