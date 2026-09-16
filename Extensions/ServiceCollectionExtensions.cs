using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace certifyAb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAllServicesAndRepos(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                foreach (var item in new[] { "Service", "Repo" })
                {
                    if (type.IsClass && !type.IsAbstract && type.Name.EndsWith(item))
                    {
                        var iFace = type.GetInterface("I" + type.Name);
                        if (iFace != null)
                            services.AddScoped(iFace, type);
                    }
                }
            }
        }
    }
}
