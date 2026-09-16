using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace certifyAb.Extensions
{
    public static class CorsSetup
    {
        public static void ConfigureCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("certifyAbPolicy", policy =>
                {

                    var origins = builder.Configuration
                        .GetSection("AllowedOrigins")
                        .Get<string[]>() ?? [];

                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }
    }
}