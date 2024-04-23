using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using Serilog;
using System.Text.Json;

namespace Rent_Motorcycle.Services
{
    //MinIO Configuration
    public class MinioConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }

    public class EntregadorService
    {
        private readonly ApplicationDbContext _context;
        private readonly MinIOService _minIOService;
        private readonly LocalStorageService _localStorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EntregadorService> _logger;
        private readonly RabbitMQSenderService _rabbitMQSender;

        public EntregadorService(ApplicationDbContext context, MinIOService minIOService, LocalStorageService localStorageService, IConfiguration configuration, ILogger<EntregadorService> logger, RabbitMQSenderService rabbitMQSender)
        {
            _context = context;
            _minIOService = minIOService;
            _localStorageService = localStorageService;
            _configuration = configuration;
            _logger = logger;
            _rabbitMQSender = rabbitMQSender;
        }

        public async Task<string> UploadImagem(byte[] imagem, string nomeArquivo = null)
        {
            try
            {
                _logger.LogInformation("Starting the Service UploadImagem of EntregadorService... - {Data}", DateTime.Now);
                string caminhoImagem;

                bool useMinIO = bool.TryParse(_configuration["UseMinIO"], out bool useMinIOValue) && useMinIOValue;

                if (useMinIO)
                {
                    // Tenta fazer upload da imagem para o MinIO
                    try
                    {
                        caminhoImagem = await _minIOService.UploadImagem(imagem);
                        _logger.LogInformation("The Image was sent using MinIO successfully. - {Data}", DateTime.Now);

                    }
                    catch (InvalidEndpointException)
                    {
                        Log.Error("For some reason it was not possible to upload using MinIO so it will be done using LocalStorage - {MinhaMsgErro}. Data: {MinhaData}", DateTime.Now);
                        caminhoImagem = await _localStorageService.UploadImagem(imagem, nomeArquivo);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error when uploading image via MinIO - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                        throw new Exception("Error when uploading image via MinIO: " + ex.Message);
                    }
                }
                else
                {
                    _logger.LogInformation("The Image was sent using LocalStorage successfully. - {Data}", DateTime.Now);
                    caminhoImagem = await _localStorageService.UploadImagem(imagem, nomeArquivo);
                }

                _logger.LogInformation("Finishing the Service UploadImagem of EntregadorService... - {Data}", DateTime.Now);
                return caminhoImagem;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred in ImageUpload - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when Image Upload: " + ex.Message);
            }
            
        }

        public async Task<bool> CadastrarEntregador(Entregador entregador)
        {
            try
            {
                _logger.LogInformation("Starting the Service CadastrarEntregador of EntregadorService... - {Data}", DateTime.Now);
                // Verifica se o CNPJ já está cadastrado
                if (await _context.Entregadores.AnyAsync(e => e.CNPJ == entregador.CNPJ))
                    throw new InvalidOperationException("Esse CNPJ já foi cadastrado.");

                // Verifica se a CNH já está cadastrada
                if (await _context.Entregadores.AnyAsync(e => e.CNH == entregador.CNH))
                    throw new InvalidOperationException("Essa CNH já foi cadastrada.");

                // Salva o entregador no banco de dados
                _context.Entregadores.Add(entregador);
                await _context.SaveChangesAsync();

                // Publica uma mensagem informando sobre um novo entregador cadastrado
                _logger.LogInformation("Publishing the addition of a new delivery person - RabittMQ - {Data}", DateTime.Now);
                try
                {
                    if (_rabbitMQSender != null && _rabbitMQSender.IsConnected)
                    {
                        var message = JsonSerializer.Serialize(entregador);
                        await _rabbitMQSender.SendMessageAsync("new-delivery-person", message);
                    }
                    else
                    {
                        _logger.LogWarning("RabbitMQ connection is not available. The message was not sent.");
                        // Aqui você pode lidar com a situação de conexão não disponível, como lançar uma exceção ou tomar alguma ação alternativa
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to send message to RabbitMQ: {ex.Message}");
                    // Lidar com o erro, como registrar no log ou lançar uma exceção
                }

                _logger.LogInformation("Finishing the CadastrarEntregador of EntregadorService... - {Data}", DateTime.Now);
                return true; // Retorna true se o cadastro for bem-sucedido
            }
            catch (Exception ex)
            {
                Log.Error("Error when registering delivery driver - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when registering delivery driver: " + ex.Message);
            }
        }

        public async Task<List<Entregador>> ConsultarEntregadores(DateTime? dataNascimento, string? nome, string? cnpj, string? cnh, string? tipoCNH)
        {
            try
            {
                _logger.LogInformation("Starting the Service ConsultarEntregadores of EntregadorService... - {Data}", DateTime.Now);

                var query = _context.Entregadores
                    .AsQueryable();

                //Filtros de pesquisa
                if (dataNascimento != null)
                    query = query.Where(e => e.DataNascimento >= dataNascimento);
                if (!string.IsNullOrEmpty(nome))
                    query = query.Where(e => e.Nome.Contains(nome));
                if (!string.IsNullOrEmpty(cnpj))
                    query = query.Where(e => e.CNPJ == cnpj);
                if (!string.IsNullOrEmpty(cnh))
                    query = query.Where(e => e.CNH == cnh);
                if (tipoCNH != null)
                    query = query.Where(e => e.TipoCNH == tipoCNH);

                var entregadores = await query.ToListAsync();

                _logger.LogInformation("Finishing the Service ConsultarEntregadores of EntregadorService... - {Data}", DateTime.Now);
                return entregadores;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when querying delivery driver - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when querying delivery driver: " + ex.Message);
            }
        }
    }

    //MinIO Service
    public class MinIOService
    {
        private readonly IMinioClient _minioClient;

        public MinIOService(MinioConfig minioConfig)
        {
            try
            {
               _minioClient = new MinioClient()
                    .WithEndpoint(minioConfig.Endpoint)
                    .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey)
                    .Build();
                _minioClient.SetAppInfo(null, "1.2.2");
            }
            catch (InvalidEndpointException)
            {
                //Ocorrendo um erro de endpoint inválido, o erro será registrado
                Log.Error("Invalid endpoint, uploading to local storage... - {MinhaMsgErro}. Data: {MinhaData}", DateTime.Now);
                Console.WriteLine("Invalid endpoint, uploading to local storage...");
            }
        }

        public async Task<string> UploadImagem(byte[] imagem)
        {
            try
            {
                string bucketName = "rent-motocycle"; //Nome do bucket criado no MinIO
                string objectName = Guid.NewGuid().ToString(); //Gerar um nome aleatório para o objeto
                
                using (var stream = new MemoryStream(imagem))
                {
                    var args = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(stream)
                        .WithContentType("application/octet-stream");

                    await _minioClient.PutObjectAsync(args);
                }

                var url = $"{bucketName}/{objectName}";
                // Retornar a URL da imagem no MinIO
                return url;
            }
            catch (Exception ex)
            {
                Log.Error("Error when Upload Image with MinIO. - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when Upload Image with MinIO: " + ex.Message);
            }
        }
    }

    public class LocalStorageService
    {
        private readonly string _storagePath;

        public LocalStorageService(IConfiguration configuration)
        {
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["LocalStorage:DirectoryName"]);
        }

        public async Task<string> UploadImagem(byte[] imagem, string nomeArquivo = null)
        {
            try
            {
                //string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _storagePath);
                string directoryPath = _storagePath;

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Concatena o nome do arquivo com o diretório para obter o caminho completo
                string filePath = Path.Combine(directoryPath, nomeArquivo);

                // Salva a imagem no armazenamento local
                await File.WriteAllBytesAsync(filePath, imagem);

                // Retorna o caminho completo da imagem no armazenamento local
                return $"{filePath}";
            }
            catch (Exception ex)
            {
                Log.Error("Error when Upload Image with MinIO. - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when Upload Image with Local Storage: " + ex.Message);
            }
        }
    }
}