using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.Interface
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

    }
}
