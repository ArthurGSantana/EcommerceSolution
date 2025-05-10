using System.Text.Json.Serialization;
using HubMinified.Api.Filters;
using HubMinified.Application.Grpc;
using HubMinified.IoC;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
DependencyContainer.RegisterServices(builder.Services, builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddGrpc();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>().AddProblemDetails();

builder.WebHost.ConfigureKestrel(options =>
{
    // Setup a HTTP/2 endpoint without TLS for development
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
    
    // Setup a HTTP/1.1 and HTTP/2 endpoint with TLS for production
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http1);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<FreightGrpcService>();

app.Run();
