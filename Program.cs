using System.Reflection;
using certifyAb.Extensions;
using certifyAb.Endpoints;
using certifyab.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAllServicesAndRepos();
builder.Services.AddAutoMapper(config => config.AddMaps(Assembly.GetExecutingAssembly()));

builder.Services.AddEndpointsApiExplorer();
builder.AddSwaggerConfiguration();
builder.ConfigureCors();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("certifyAbPolicy");

app.MapCertificateEndpoints();
app.Run();
