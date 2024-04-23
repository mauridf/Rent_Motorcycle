using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rent_Motorcycle.Tests
{
    public class MotoServiceTests
    {
        [Fact]
        public async Task CadastrarMoto_ValidMoto_ReturnsTrue()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockLogger = new Mock<ILogger<MotoService>>();
            var mockRabbitMQSender = new Mock<RabbitMQSenderService>();

            var service = new MotoService(mockContext.Object, mockLogger.Object, mockRabbitMQSender.Object);
            var moto = new Moto { Placa = "ABC123" };

            mockContext.Setup(m => m.Motos.AnyAsync(It.IsAny<Expression<Func<Moto, bool>>>(), default)).ReturnsAsync(false);

            // Act
            var result = await service.CadastrarMoto(moto);

            // Assert
            Assert.True(result);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task ConsultarMotos_WithPlaca_ReturnsListOfMotos()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockLogger = new Mock<ILogger<MotoService>>();
            var mockRabbitMQSender = new Mock<RabbitMQSenderService>();

            var service = new MotoService(mockContext.Object, mockLogger.Object, mockRabbitMQSender.Object);
            var motos = new List<Moto> { new Moto { Placa = "ABC123" }, new Moto { Placa = "DEF456" } };

            mockContext.Setup(m => m.Motos).Returns(DbSetMock.GetQueryableMockDbSet(motos));

            // Act
            var result = await service.ConsultarMotos("ABC123");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ABC123", result.First().Placa);
        }

        [Fact]
        public async Task ModificarMoto_WithValidIdAndPlaca_ReturnsTrue()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockLogger = new Mock<ILogger<MotoService>>();
            var mockRabbitMQSender = new Mock<RabbitMQSenderService>();

            var service = new MotoService(mockContext.Object, mockLogger.Object, mockRabbitMQSender.Object);
            var moto = new Moto { Id = 1, Placa = "ABC123" };

            mockContext.Setup(m => m.Motos.FindAsync(1)).ReturnsAsync(moto);
            mockContext.Setup(m => m.Motos.AnyAsync(It.IsAny<Expression<Func<Moto, bool>>>(), default)).ReturnsAsync(false);

            // Act
            var result = await service.ModificarMoto(1, "XYZ789");

            // Assert
            Assert.True(result);
            Assert.Equal("XYZ789", moto.Placa);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task RemoverMoto_WithValidIdAndNoRentals_ReturnsTrue()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockLogger = new Mock<ILogger<MotoService>>();
            var mockRabbitMQSender = new Mock<RabbitMQSenderService>();

            var service = new MotoService(mockContext.Object, mockLogger.Object, mockRabbitMQSender.Object);
            var moto = new Moto { Id = 1 };

            mockContext.Setup(m => m.Motos.FindAsync(1)).ReturnsAsync(moto);
            mockContext.Setup(m => m.Locacoes.AnyAsync(It.IsAny<Expression<Func<Locacao, bool>>>(), default)).ReturnsAsync(false);

            // Act
            var result = await service.RemoverMoto(1);

            // Assert
            Assert.True(result);
            mockContext.Verify(m => m.Motos.Remove(It.IsAny<Moto>()), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }
    }

    // Classe utilitária para mockar um DbSet do Entity Framework
    public static class DbSetMock
    {
        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet.Object;
        }
    }
}