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

        public EntregadorService(ApplicationDbContext context, MinIOService minIOService, LocalStorageService localStorageService, IConfiguration configuration, ILogger<EntregadorService> logger)
        {
            _context = context;
            _minIOService = minIOService;
            _localStorageService = localStorageService;
            _configuration = configuration;
            _logger = logger;
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
                        Log.Error("For some reason it was not possible to upload using MinIO so it will be done using LocalStorage - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
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

                _logger.LogInformation("Finishing the CadastrarEntregador of EntregadorService... - {Data}", DateTime.Now);
                return true; // Retorna true se o cadastro for bem-sucedido
            }
            catch (Exception ex)
            {
                Log.Error("Error when registering delivery driver - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when registering delivery driver: " + ex.Message);
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
                // Se ocorrer um erro de endpoint inválido, apenas registre o erro
                Console.WriteLine("Endpoint inválido, fazendo upload para o armazenamento local...");
            }
        }

        public async Task<string> UploadImagem(byte[] imagem)
        {
            string bucketName = "rent-motocycle"; // Nome do bucket criado no MinIO
            string objectName = Guid.NewGuid().ToString(); // Gerar um nome aleatório para o objeto

            using (var stream = new MemoryStream(imagem))
            {
                var args = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithContentType("application/octet-stream");

                await _minioClient.PutObjectAsync(args);
            }

            // Retornar a URL da imagem no MinIO
            return $"{bucketName}/{objectName}";
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
    }
}