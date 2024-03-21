using Toolbelt.Blazor;

namespace GiamSat.UI
{
    public interface IHttpInterceptorManager
    {
        void RegisterEvent();
        Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs args);
        void DisposeEvent();
    }
}
