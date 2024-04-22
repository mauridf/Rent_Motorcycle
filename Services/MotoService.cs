using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Serilog;

namespace Rent_Motorcycle.Services
{
    public class MotoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MotoService> _logger;

        public MotoService(ApplicationDbContext context, ILogger<MotoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CadastrarMoto(Moto moto)
        {
            try
            {
                _logger.LogInformation("Starting the Service CadastrarMoto of MotoService... - {Data}", DateTime.Now);
                // Verifica se a placa já está cadastrada
                if (await _context.Motos.AnyAsync(m => m.Placa == moto.Placa))
                    throw new InvalidOperationException("This license plate has already been registered.");

                // Salva a moto no banco de dados
                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Finishing the Service CadastrarMoto of MotoService... - {Data}", DateTime.Now);
                return true; // Retorna true se o cadastro for bem-sucedido
            }
            catch (Exception ex)
            {
                Log.Error("Error when registering license plate - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when registering license plate: " + ex.Message);
            }
        }
    }
}
