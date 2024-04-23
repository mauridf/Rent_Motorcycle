using Microsoft.EntityFrameworkCore;
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
    public class MotoServiceTests
    {
        private readonly ApplicationDbContext _context;

        public MotoServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Database")
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CadastrarMoto_Success()
        {
            //Arrange
            var mockDbSet = new Mock<DbSet<Moto>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Motos).Returns(mockDbSet.Object);
            var motoService = new MotoService(mockContext.Object, null, null);
            var moto = new Moto
            {
                Ano = 2022,
                Modelo = "Moto de Teste",
                Placa = "XYZ1234"
            };

            //Act
            var result = await motoService.CadastrarMoto(moto);

            //Assert
            Assert.True(result);
            mockDbSet.Verify(m => m.Add(It.IsAny<Moto>()), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(), Times.Once);
        }
    }
}
