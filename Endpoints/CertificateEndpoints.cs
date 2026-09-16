using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using certifyab.Core.Interfaces;
using certifyAb.Core.Dto;

namespace certifyAb.Endpoints
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
            .WithSummary("Create a certificate – requires receiver, course and date. Returns Id and Url.");


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
            .WithSummary("Get a certificate by Id.");


            // Lista alla certifikat (kräver API-nyckel i header)
            app.MapGet("/certificates", async (ICertificateService _service) =>
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
            .WithSummary("Get all certificates as a list.");

            // Publik verifierings-endpoint — returnerar äkthetsbevis
            app.MapGet("/verify/{uuid}", () =>
            {
                throw new NotImplementedException();
            })
            .WithName("Get Certificate Of Authenticity")
            .WithSummary("Returns Certificate Of Authenticity for the selected certificate.");


            // Health check
            app.MapGet("/health", () =>
            {
                throw new NotImplementedException();
            })
            .WithName("Get Health Check")
            .WithSummary("Returns a Health Check.");
        }
    }
}