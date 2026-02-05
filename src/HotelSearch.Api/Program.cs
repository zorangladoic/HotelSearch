using FluentValidation;
using HotelSearch.Api.Extensions;
using HotelSearch.Api.Middleware;
using HotelSearch.Application.Common;
using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Hotel Search API", Version = "v1" });
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("SearchResults", b =>
        b.Expire(TimeSpan.FromMinutes(2))
         .SetVaryByQuery("latitude", "longitude", "page", "pageSize"));
});

builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Search API v1"));
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseOutputCache();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
