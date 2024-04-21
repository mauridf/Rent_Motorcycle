using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Services;
using Rent_Motorcycle.Utils;
using Serilog;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configurar o logger Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Adicionar Serilog como provedor de log

// Add services to the container.
builder.Services.AddControllers();

// Configure o DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure MinioConfig
builder.Services.Configure<MinioConfig>(builder.Configuration.GetSection("MinioConfig"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<MinioConfig>>().Value);

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Rent_Motorcycle", Version = "v1" });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Rent_Motorcycle.xml"));

    // Adicionar suporte a upload de arquivos
    c.DocumentFilter<FormFileUploadDocumentFilter>();
    c.OperationFilter<FileUploadOperationFilter>();
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "file", Format = "binary" });
});

// Add endpoints for API explorer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add scoped services
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<MinIOService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<EntregadorService>();
builder.Services.AddScoped<LocacaoService>();

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

app.Run();