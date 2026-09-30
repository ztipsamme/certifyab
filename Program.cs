using System.Reflection;
using certifyab.Extensions;
using certifyab.Endpoints;
using certifyab.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

if (!builder.Environment.IsDevelopment())
    builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddAllServicesAndRepos();
builder.Services.AddAutoMapper(config => config.AddMaps(Assembly.GetExecutingAssembly()));

builder.Services.AddEndpointsApiExplorer();
builder.AddSwaggerConfiguration();
builder.ConfigureCors();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("certifyabPolicy");

if (!builder.Environment.IsDevelopment())
    app.UseMiddleware<ErrorMetricMiddleware>();

app.MapCertificateEndpoints();
app.Run();
