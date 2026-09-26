using System.Reflection;
using certifyab.Extensions;
using certifyab.Endpoints;

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
app.UseCors("certifyabPolicy");

app.MapCertificateEndpoints();
app.Run();
