using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rent_Motorcycle.tests
{
    public class EntregadorServiceTests
    {
        private readonly IConfiguration _configuration;

        public EntregadorServiceTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [Fact]
        public async Task UploadImagem_MinIO_Success()
        {
            //Arrange
            var minioConfig = new MinioConfig
            {
                Endpoint = "http://localhost:9000",
                AccessKey = "minio",
                SecretKey = "minio123"
            };
            var minIOService = new MinIOService(minioConfig);
            var localStorageService = new LocalStorageService(_configuration);
            var entregadorService = new EntregadorService(null, minIOService, localStorageService, _configuration, null, null);
            byte[] imagem = File.ReadAllBytes("test_image.png");

            //Act
            var url = await entregadorService.UploadImagem(imagem);

            //Assert
            Assert.NotNull(url);
            Assert.StartsWith("rent-motocycle", url);
        }

        [Fact]
        public async Task UploadImagem_LocalStorage_Success()
        {
            //Arrange
            var minioConfig = new MinioConfig
            {
                Endpoint = "http://invalidendpoint:9000",
                AccessKey = "minio",
                SecretKey = "minio123"
            };
            var minIOService = new MinIOService(minioConfig);
            var localStorageService = new LocalStorageService(_configuration);
            var entregadorService = new EntregadorService(null, minIOService, localStorageService, _configuration, null, null);
            byte[] imagem = File.ReadAllBytes("test_image.png");

            //Act
            var url = await entregadorService.UploadImagem(imagem);

            //Assert
            Assert.NotNull(url);
            Assert.EndsWith(".png", url);
        }

        [Fact]
        public async Task CadastrarEntregador_Success()
        {
            //Arrange
            var mockDbSet = new Mock<DbSet<Entregador>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Entregadores).Returns(mockDbSet.Object);
            var entregadorService = new EntregadorService(mockContext.Object, null, null, _configuration, null, null);
            var entregador = new Entregador
            {
                Nome = "Fulano Beltrano Ciclano",
                CNPJ = "12345678901234",
                DataNascimento = DateTime.Now.AddYears(-25),
                CNH = "123456789012",
                TipoCNH = "A",
                ImagemCNHUrl = "http://example.com/cnh.png"
            };

            //Act
            var result = await entregadorService.CadastrarEntregador(entregador);

            //Definir variáveis locais para os argumentos opcionais
            var anyEntregador = It.IsAny<Entregador>();

            //Assert
            Assert.True(result);
            mockDbSet.Verify(m => m.Add(anyEntregador), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(), Times.Once);
        }
    }
}
