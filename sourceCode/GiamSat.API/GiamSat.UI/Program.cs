using Blazored.LocalStorage;
using GiamSat.APIClient;
using GiamSat.Models;
using GiamSat.UI;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Globalization;
using GiamSat.UI.Authorization;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var config = builder.Configuration;

GlobalVariable.RefreshInterval = int.TryParse(config["AppSettings:RefreshInterval"].ToString(),out int value) ? value : 1000;
GlobalVariable.ChartRefreshInterval = int.TryParse(config["AppSettings:ChartRefreshInterval"].ToString(),out value) ? value : 1000;
GlobalVariable.ChartPointNum = int.TryParse(config["AppSettings:ChartPointNum"].ToString(),out value) ? value : 10;

builder.Services.AddMudServices();
builder.Services.AddBlazorBootstrap();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AutoRegisterInterfaces<IApiService>();

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(UserRoles.Admin, policy =>
    {
        policy.RequireRole(UserRoles.Admin);
        policy.RequireRole(UserRoles.User);
    });
});

builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationService>()
    .AddScoped(sp => (JwtAuthenticationService)sp.GetRequiredService<AuthenticationStateProvider>())
    .AddScoped(sp => (IAccessTokenProvider)sp.GetRequiredService<AuthenticationStateProvider>())
    .AddScoped<IAccessTokenProviderAccessor, AccessTokenProviderAccessor>()
    .AddScoped<JwtAuthenticationHeaderHandler>();

builder.Services.AddScoped<IHttpInterceptorManager, HttpInterceptorManager>();

//add them client local
builder.Services.AddHttpClient("local", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

//add client API
builder.Services.AddHttpClient("GiamSatAPI", (sp, client) =>
{
    client.DefaultRequestHeaders.AcceptLanguage.Clear();
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
    client.BaseAddress = new Uri(config["AppSettings:ApiBaseUrl"]);
    client.EnableIntercept(sp);
}).AddHttpMessageHandler<JwtAuthenticationHeaderHandler>().Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GiamSatAPI"));
builder.Services.AddHttpClientInterceptor();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddTransient<ISDisplayRealtime, DisplayRealtimeApiClient>();
builder.Services.AddTransient<ISChuongInfo, ChuongInfoApiClient>();
//builder.Services.AddScoped<Common>();

#region set use dot as decimal separator when the current culture is Greek
// Define the list of cultures your app will support 
var supportedCultures = new List<CultureInfo>()
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("es-ES"),
                    new CultureInfo("de-DE")
                };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Set the default culture 
    var culture = new CultureInfo("de-DE");
    //culture.NumberFormat.NumberDecimalSeparator = ".";
    options.DefaultRequestCulture = new RequestCulture(culture);

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>() {
                 new QueryStringRequestCultureProvider() // Here, You can also use other localization provider 
                };
});

//var supportedCultures = new[]
//{
//    new System.Globalization.CultureInfo("de-DE"),
//};


//builder.Services.Configure<RequestLocalizationOptions>(options =>
//{
//    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("de-DE");
//    options.SupportedCultures = supportedCultures;
//    options.SupportedUICultures = supportedCultures;
//});
#endregion

await builder.Build().RunAsync();


