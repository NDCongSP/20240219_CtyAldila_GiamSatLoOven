using GiamSat.Models;
using RestEase;
using System.Net.Http;

namespace GiamSat.UI
{
    public static class Extensions
    {
        public static IServiceCollection AutoRegisterInterfaces<T>(this IServiceCollection services)
        {
            var @interface = typeof(T);

            var types = @interface
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types)
            {
                if (@interface.IsAssignableFrom(type.Service))
                {
                    services.AddTransient(type.Service, type.Implementation);
                }
            }
            return services;
        }

        /// <summary>
        /// Đăng ký các RestEase client interfaces. Dùng chung HttpClient (đã có JWT handler)
        /// đã được register Scoped trong Program.cs (factory CreateClient("GiamSatAPI")).
        /// </summary>
        public static IServiceCollection AddRestEaseClients(this IServiceCollection services)
        {
            services.AddScoped<ILogsClient>(sp =>
                RestClient.For<ILogsClient>(sp.GetRequiredService<HttpClient>()));
            return services;
        }
    }
}
