using certifyab.Core.Interfaces;
using certifyab.Extensions;
using certifyab.Core.Dto;

namespace certifyab.Endpoints
{
    public static class CertificateEndpoints
    {
        public static void MapCertificateEndpoints(this WebApplication app)
        {
            // Skapa certifikat (mottagare, kurs, datum), returnera ID + URL
            app.MapPost("/certificates", async (HttpRequest req, ICertificateService _service) =>
            {
                try
                {
                    var dto = await req.ReadFromJsonAsync<CertificateCreateDTO>();

                    if (dto is null)
                        return Results.BadRequest("Request body is required.");

                    var certificate = await _service.CreateAsync(dto);

                    return Results.Created($"/certificates/{certificate.Id}", certificate);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

            })
            .WithName("Create Certificate")
            .WithSummary("Create a new certificate")
            .WithTags("Certificates")
            .RequireApiKey()
            .Produces<CertificateCreatedDTO>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest);


            // Hämta certifikatdata (JSON)
            app.MapGet("/certificates/{id}", async (string id, ICertificateService _service) =>
            {
                try
                {
                    var certificate = await _service.GetByIdAsync(id);
                    return Results.Ok(certificate);
                }
                catch
                {
                    return Results.NotFound();
                }
            })
            .WithName("Get Certificate By Id")
            .WithSummary("Get a certificate by Id.")
            .WithTags("Certificates")
            .RequireApiKey()
            .Produces<CertificateDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);


            // Lista alla certifikat (kräver API-nyckel i header)
            app.MapGet("/certificates", async (HttpRequest req, ICertificateService _service, IConfiguration config) =>
            {
                try
                {
                    var certificates = await _service.GetAllAsync();

                    return Results.Ok(certificates);
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.ToString(), statusCode: 500);
                }
            })
            .WithName("Get All Certificates")
            .WithSummary("Get all certificates as a list.")
            .WithTags("Certificates")
            .RequireApiKey()
            .Produces<List<CertificateDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

            // Publik verifierings-endpoint — returnerar äkthetsbevis
            app.MapGet("/verify/{uuid}", async (string uuid, ICertificateService _service) =>
                {
                    try
                    {
                        var certificate = await _service.GetByUuidAsync(uuid);
                        return Results.Ok(certificate);
                    }
                    catch
                    {
                        return Results.NotFound();
                    }
                })
                .WithName("Get Certificate Of Authenticity")
                .WithSummary("Returns Certificate Of Authenticity for the selected certificate.")
                .WithTags("Certificates")
                .Produces<CertificatePublicDTO>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);


            // Health check
            app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("Get Health Check")
            .WithSummary("Returns a Health Check.")
            .WithTags("Health")
            .Produces<string>(StatusCodes.Status200OK);
        }
    }
}