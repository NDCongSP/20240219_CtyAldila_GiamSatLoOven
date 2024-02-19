
using Blazored.LocalStorage;
using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GiamSat.Interface.Authorization
{
    public class JwtAuthenticationService : AuthenticationStateProvider, IAccessTokenProvider
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly ILocalStorageService _localStorage;
        // private readonly IPersonalClient _personalClient;
        private readonly IAuthenticateClient _tokenClient;
        private readonly NavigationManager _navigation;

        public JwtAuthenticationService(
            ILocalStorageService localStorage,
            IAuthenticateClient tokenClient,
            NavigationManager navigation)
        {
            _localStorage = localStorage;
            _tokenClient = tokenClient;
            _navigation = navigation;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string cachedToken = await GetCachedAuthTokenAsync();
            if (string.IsNullOrWhiteSpace(cachedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Generate claimsIdentity from cached token
            var claimsIdentity = new ClaimsIdentity(GetClaimsFromJwt(cachedToken), "jwt");

            foreach (var item in claimsIdentity.Claims.ToList())
            {
                Console.WriteLine(item.Type + "Value = " + item.Value);
            }


            // Add cached permissions as claims
            //if (await GetCachedPermissionsAsync() is List<string> cachedPermissions)
            //{
            //    claimsIdentity.AddClaims(cachedPermissions.Select(p => new Claim(FramasClaims.Permission, p)));
            //}

            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);
            if (claimPrincipal.Identity.IsAuthenticated)
            {

            }

            string text = "[\"Admin\", \"User\"]";
            if (claimPrincipal.IsInRole(text))
            {
                Console.WriteLine("Co quyen admin");
            }


            return new AuthenticationState(claimPrincipal);
        }

        public void NavigateToExternalLogin(string returnUrl) =>
            throw new NotImplementedException();

        public async Task<LoginResult> LoginAsync(LoginModel request)
        {
            var tokenResponse = await _tokenClient.LoginAsync(request);

            string token = tokenResponse.Token;
            string refreshToken = "";
            string sessionId = "";
            //string refreshToken = tokenResponse.RefreshToken;
            //string sessionId = tokenResponse.SessionId;

            if (string.IsNullOrWhiteSpace(token))
            {
                return null ;
            }

            await CacheAuthTokens(token, refreshToken, sessionId);

            var claimsIdentity = new ClaimsIdentity(GetClaimsFromJwt(token), "jwt");

            //// Get permissions for the current user and add them to the cache
            //var permissions = claimsIdentity.FindAll(FramasClaims.Permission).Select(x => x.Value).ToList();

            //await CachePermissions(permissions);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return tokenResponse;
        }

        public async Task LogoutAsync()
        {
            await ClearCacheAsync();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            //_navigation.NavigateTo(_authSettings.Value.LoginUrl);
            _navigation.NavigateTo("/login");
        }

        public async Task ReLoginAsync(string returnUrl)
        {
            await LogoutAsync();
            _navigation.NavigateTo(returnUrl);
        }

        public async ValueTask<AccessTokenResult> RequestAccessToken()
        {
            var authState = await GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated is not true)
            {
                return new AccessTokenResult(AccessTokenResultStatus.RequiresRedirect, null, "/login");
            }

            // We make sure the access token is only refreshed by one thread at a time. The other ones have to wait.
            await _semaphore.WaitAsync();
            try
            {
                string token = await GetCachedAuthTokenAsync();


                // Check if token needs to be refreshed (when its expiration time is less than 1 minute away)
                var expTime = GetExpiration(authState.User);
                var diff = expTime - DateTime.UtcNow;
                if (diff.TotalMinutes <= 10)
                {
                    //string refreshToken = await GetCachedRefreshTokenAsync();
                    //(bool succeeded, var response) = await TryRefreshTokenAsync(new RefreshTokenRequest { Token = token, RefreshToken = refreshToken });
                    //if (!succeeded)
                    //{
                    //    return new AccessTokenResult(AccessTokenResultStatus.RequiresRedirect, null, _authSettings.Value.LoginUrl);
                    //}

                    //token = response?.Token;
                }
                else if (diff.TotalMinutes < 0)
                {
                    return new AccessTokenResult(AccessTokenResultStatus.RequiresRedirect, new AccessToken() { Value = null  }, "/login");
                }

                return new AccessTokenResult(AccessTokenResultStatus.Success, new AccessToken() { Value = token }, string.Empty);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options) =>
            RequestAccessToken();

        //private async Task<(bool Succeeded, TokenResponse Token)> TryRefreshTokenAsync(RefreshTokenRequest request)
        //{
        //    var authState = await GetAuthenticationStateAsync();
        //    string tenantKey = authState.User.GetUserId();
        //    if (string.IsNullOrWhiteSpace(tenantKey))
        //    {
        //        throw new InvalidOperationException("Can't refresh token when user is not logged in!");
        //    }

        //    try
        //    {
        //        //var tokenResponse = await _identityClient.GetRefreshTokenAsync(request);

        //        //await CacheAuthTokens(tokenResponse.Data.Token, tokenResponse.Data.RefreshToken);

        //        //return (true, tokenResponse.Data);
        //    }
        //    catch // (ApiException<ErrorResult>)
        //    {
        //    }
        //    return (false, null);
        //}

        private async ValueTask CacheAuthTokens(string token, string refreshToken, string sessionId)
        {
            await _localStorage.SetItemAsync(StorageConts.AuthToken, token);
            //await _localStorage.SetItemAsync(StorageConstants.Local.RefreshToken, refreshToken);
            //await _localStorage.SetItemAsync(StorageConstants.Local.SessionId, sessionId);
        }

        private ValueTask CachePermissions(ICollection<string> permissions) =>
            _localStorage.SetItemAsync(StorageConts.Permission, permissions);

        private async Task ClearCacheAsync()
        {
            await _localStorage.RemoveItemAsync(StorageConts.AuthToken);
            //await _localStorage.RemoveItemAsync(StorageConstants.Local.RefreshToken);
            //await _localStorage.RemoveItemAsync(StorageConstants.Local.SessionId);
            //await _localStorage.RemoveItemAsync(StorageConstants.Local.Permissions);
        }

        private ValueTask<string> GetCachedAuthTokenAsync() =>
            _localStorage.GetItemAsync<string>(StorageConts.AuthToken);

        private ValueTask<string> GetCachedRefreshTokenAsync() =>
            _localStorage.GetItemAsync<string>(StorageConts.RefreshToken);

        private ValueTask<ICollection<string>> GetCachedPermissionsAsync() =>
            _localStorage.GetItemAsync<ICollection<string>>(StorageConts.Permission);

        private IEnumerable<Claim> GetClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            if (keyValuePairs is not null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    Console.WriteLine(kvp.Key);
                    Console.WriteLine(kvp.Value.ToString() ?? string.Empty);
                    Console.WriteLine(kvp.Value.GetType());

                    if (kvp.Value is JsonElement elm)
                    {
                        if (elm.ValueKind == JsonValueKind.Array)
                        {
                            var enume = elm.EnumerateArray();
                            while (enume.MoveNext())
                            {
                                claims.Add(new Claim(kvp.Key, enume.Current.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
                        }
                    }

                    // claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));

                    //if (kvp.Key == ClaimRoleValue)
                    //{
                    //    List<string> roles = JsonSerializer.Deserialize<List<string>>(kvp.Value.ToString());
                    //    foreach (var role in roles)
                    //    {
                    //        claims.Add(new Claim(ClaimTypes.Role, role));
                    //    }
                    //}

                }
                // claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)));

                //foreach (var item in claimsIdentity.Claims.ToList())
                //{
                //    Console.WriteLine(item.Type + "Value = " + item.Value);
                //    if (item.Type == ClaimRoleValue)
                //    {
                //        List<string> roles = JsonSerializer.Deserialize<List<string>>(item.Value);
                //        foreach (var role in roles)
                //        {
                //            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                //        }
                //    }
                //}
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string payload)
        {
            payload = payload.Trim().Replace('-', '+').Replace('_', '/');
            string base64 = payload.PadRight(payload.Length + ((4 - (payload.Length % 4)) % 4), '=');
            return Convert.FromBase64String(base64);
        }

        public static DateTimeOffset GetExpiration(ClaimsPrincipal principal)
        {
            return DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(principal.FindFirst("exp")?.Value));
        }
    }
}
