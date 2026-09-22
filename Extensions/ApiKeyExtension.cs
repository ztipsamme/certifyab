using static certifyab.Extensions.SwaggerSetup;

namespace certifyab.Extensions
{
    public static class ApiKeyExtension
    {
        public static RouteHandlerBuilder RequireApiKey(this RouteHandlerBuilder builder)
        {
            builder.WithMetadata(new ApiKeyRequiredMetadata());

            return builder.AddEndpointFilter(async (context, next) =>
            {
                var config = context.HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>();

                var expectedApiKey = config["ApiKey"];
                var providedApiKey = context.HttpContext.Request.Headers["X-API-Key"].FirstOrDefault();

                if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != expectedApiKey)
                    return Results.Unauthorized();

                return await next(context);
            });
        }
    }
}