using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rent_Motorcycle.tests
{
    public class LocacaoServiceTests
    {
        private readonly ApplicationDbContext _context;

        public LocacaoServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Database")
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public void CalcularValorTotalLocacao_Success()
        {
            //Arrange
            var locacaoService = new LocacaoService(_context, null, null);
            var dataInicio = DateTime.Today.AddDays(1);
            var tipoPlanoId = 1;
            var dataTermino = DateTime.Today.AddDays(8);

            //Act
            var valorTotal = locacaoService.CalcularValorTotalLocacao(dataInicio, tipoPlanoId, dataTermino);

            //Assert
            Assert.Equal(400m, valorTotal);
        }
    }
}
