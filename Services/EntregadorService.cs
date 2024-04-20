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

namespace Rent_Motorcycle.Services
{
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

        public EntregadorService(ApplicationDbContext context, MinIOService minIOService, LocalStorageService localStorageService, IConfiguration configuration)
        {
            _context = context;
            _minIOService = minIOService;
            _localStorageService = localStorageService;
            _configuration = configuration;
        }

        public async Task<string> UploadImagem(byte[] imagem, string nomeArquivo = null)
        {
            string caminhoImagem;

            bool useMinIO = bool.TryParse(_configuration["UseMinIO"], out bool useMinIOValue) && useMinIOValue;

            if (useMinIO)
            {
                // Tenta fazer upload da imagem para o MinIO
                try
                {
                    caminhoImagem = await _minIOService.UploadImagem(imagem);
                }
                catch (InvalidEndpointException)
                {
                    // Se falhar devido a um endpoint inválido, faz upload da imagem para o armazenamento local
                    caminhoImagem = await _localStorageService.UploadImagem(imagem, nomeArquivo);
                }
                catch (Exception ex)
                {
                    // Se houver outro erro, lança uma exceção
                    throw new Exception("Erro ao fazer upload da imagem: " + ex.Message);
                }
            }
            else
            {
                // Faz upload da imagem para o armazenamento local
                caminhoImagem = await _localStorageService.UploadImagem(imagem, nomeArquivo);
            }

            return caminhoImagem;
        }

        public async Task<bool> CadastrarEntregador(Entregador entregador)
        {
            // Verifica se o CNPJ já está cadastrado
            if (await _context.Entregadores.AnyAsync(e => e.CNPJ == entregador.CNPJ))
                throw new InvalidOperationException("Esse CNPJ já foi cadastrado.");

            // Verifica se a CNH já está cadastrada
            if (await _context.Entregadores.AnyAsync(e => e.CNH == entregador.CNH))
                throw new InvalidOperationException("Essa CNH já foi cadastrada.");

            // Salva o entregador no banco de dados
            _context.Entregadores.Add(entregador);
            await _context.SaveChangesAsync();

            return true; // Retorna true se o cadastro for bem-sucedido
        }
    }

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