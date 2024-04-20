using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Services;
using Rent_Motorcycle.Utils;

var builder = WebApplication.CreateBuilder(args);

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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