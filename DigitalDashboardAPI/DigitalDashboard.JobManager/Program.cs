using DigitalDashboard.DAL.Models;
using DigitalDashboard.JobManager.Repository;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_MyCorsPolicy";

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
builder.Services.AddSingleton<IDataImportRepository, DataImportRepository>();
builder.Services.AddSingleton<IDataExportRepository, DataExportRepository>();
builder.Services.AddSingleton<ILogRepository, LogRepository>();
builder.Services.AddSingleton<DataValidation>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

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
