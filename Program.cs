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
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Events;
using Rent_Motorcycle.Services.RabbitMQ;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

//Configurar o Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); //Adicionar o Serilog como provedor de log

//Add services to the container.
builder.Services.AddControllers();

// Configure o DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//Configure MinioConfig
builder.Services.Configure<MinioConfig>(builder.Configuration.GetSection("MinioConfig"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<MinioConfig>>().Value);

IConnection connection = null;

// Configure RabbitMQ
try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
    var rabbitMQConfig = configuration.GetSection("RabbitMQ");

    var factory = new ConnectionFactory
    {
        HostName = rabbitMQConfig["Host"],
        Port = int.Parse(rabbitMQConfig["Port"]),
        VirtualHost = rabbitMQConfig["VirtualHost"],
        UserName = rabbitMQConfig["Username"],
        Password = rabbitMQConfig["Password"]
    };
    connection = factory.CreateConnection();
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao conectar-se ao RabbitMQ: {ex.Message}");
}

if (connection != null)
{
    // Registra a conexão no contêiner de dependências apenas se a conexão for bem-sucedida
    builder.Services.AddSingleton<IConnection>(connection);

    // Configure RabbitMQ consumer
    var channel = connection.CreateModel();
    channel.QueueDeclare(queue: "nova-locacao",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Lógica para processar a mensagem (por exemplo, enviar uma notificação)
            var locacao = JsonSerializer.Deserialize<LocacaoService>(message);
            // Envie uma notificação sobre a locação

            // Confirma o recebimento da mensagem
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // Trate o erro ao processar a mensagem
            Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
        }
    };
    channel.BasicConsume(queue: "nova-locacao",
                         autoAck: false,
                         consumer: consumer);
}

//Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Rent_Motorcycle", Version = "v1" });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Rent_Motorcycle.xml"));

    //Adicionar suporte a upload de arquivos
    c.DocumentFilter<FormFileUploadDocumentFilter>();
    c.OperationFilter<FileUploadOperationFilter>();
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "file", Format = "binary" });
});

//Add endpoints for API explorer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Se a conexão com o RabbitMQ falhar, registre uma implementação padrão nula de IConnection.
//Foi criada uma Classe apenas com esse intuito para que a aplicação possa iniciar sem problema na falta ou falha do RabbitMQ
if (connection == null)
{
    builder.Services.AddSingleton<IConnection>(new NullConnection());
}

//Add scoped services
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<MinIOService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<EntregadorService>();
builder.Services.AddScoped<LocacaoService>();
builder.Services.AddSingleton<RabbitMQSenderService>();
builder.Services.AddSingleton<RabbitMQConsumerService>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();