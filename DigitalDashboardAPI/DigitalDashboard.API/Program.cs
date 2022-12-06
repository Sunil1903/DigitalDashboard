using DigitalDashboard.BLL.Authorization;
using DigitalDashboard.BLL.Interfaces;
using DigitalDashboard.BLL.Repository;
using DigitalDashboard.DAL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var MyAllowSpecificOrigins = "_MyCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Set policy for CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.Configure<DigitalDashboardDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DigitalDashboardDatabaseSettings)));

builder.Services.AddSingleton<IDigitalDashboardDatabaseSettings>(sp =>
sp.GetRequiredService<IOptions<DigitalDashboardDatabaseSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(s =>
        new MongoClient(builder.Configuration.GetValue<string>("DigitalDashboardDatabaseSettings:ConnectionString")));

builder.Services.AddSingleton<IRegulationsByCountryRepository, RegulationsByCountryRepository>();
builder.Services.AddSingleton<IRegulatorySKURepository,RegulatorySKURepository>();
builder.Services.AddSingleton<IUserAuthorization, UserAuthorization>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
