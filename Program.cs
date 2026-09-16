using System.Reflection;
using certifyAb.Extensions;
using certifyAb.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAllServicesAndRepos();
builder.Services.AddAutoMapper(config => config.AddMaps(Assembly.GetExecutingAssembly()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.ConfigureCors();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("certifyAbPolicy");

var isDev = app.Environment.IsDevelopment();

app.MapCertificateEndpoints(isDev);
app.Run();
