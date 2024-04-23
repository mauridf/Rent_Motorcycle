using System.Linq;
using System.Text.Json;
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
        private readonly RabbitMQSenderService _rabbitMQSender;

        public MotoService(ApplicationDbContext context, ILogger<MotoService> logger, RabbitMQSenderService rabbitMQSender)
        {
            _context = context;
            _logger = logger;
            _rabbitMQSender = rabbitMQSender;
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

                // Publica uma mensagem informando sobre uma nova inclusão de Moto
                _logger.LogInformation("Publishing the addition of a new bike - RabittMQ - {Data}", DateTime.Now);
                try
                {
                    if (_rabbitMQSender != null && _rabbitMQSender.IsConnected)
                    {
                        var message = JsonSerializer.Serialize(moto);
                        await _rabbitMQSender.SendMessageAsync("new-motorcycle", message);
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

                _logger.LogInformation("Finishing the Service CadastrarMoto of MotoService... - {Data}", DateTime.Now);
                return true; // Retorna true se o cadastro for bem-sucedido
            }
            catch (Exception ex)
            {
                Log.Error("Error when registering license plate - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when registering license plate: " + ex.Message);
            }
        }

        public async Task<List<Moto>> ConsultarMotos(string placa)
        {
            try
            {
                _logger.LogInformation("Starting the Service ConsultarMotos of MotoService... - {Data}", DateTime.Now);
                IQueryable<Moto> query = _context.Motos;

                if (!string.IsNullOrEmpty(placa))
                {
                    query = query.Where(m => m.Placa == placa);
                }

                var motos = await query.ToListAsync();
                _logger.LogInformation("Finishing the Service ConsultarMotos of MotoService... - {Data}", DateTime.Now);
                return motos;
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying motorcycles - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when querying motorcycles: " + ex.Message);
            }
        }

        public async Task<bool> ModificarMoto(int id, string novaPlaca)
        {
            try
            {
                _logger.LogInformation("Starting the Service ModificarMoto of MotoService... - {Data}", DateTime.Now);
                var moto = await _context.Motos.FindAsync(id);
                if (moto == null)
                    throw new KeyNotFoundException($"Motorcycle with ID {id} not found.");

                var motoExistsWithNewPlaca = await _context.Motos.AnyAsync(m => m.Placa == novaPlaca);
                if (motoExistsWithNewPlaca)
                    throw new InvalidOperationException("A motorcycle with the new license plate already exists.");

                moto.Placa = novaPlaca;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Finishing the Service ModificarMoto of MotoService... - {Data}", DateTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error when modifying motorcycle - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when modifying motorcycle: " + ex.Message);
            }
        }

        public async Task<bool> RemoverMoto(int id)
        {
            try
            {
                _logger.LogInformation("Starting the Service RemoverMoto of MotoService... - {Data}", DateTime.Now);
                var moto = await _context.Motos.FindAsync(id);
                if (moto == null)
                    throw new KeyNotFoundException($"Motorcycle with ID {id} not found.");

                // Verifica se há locações associadas à moto
                var hasRentals = await _context.Locacoes.AnyAsync(l => l.MotoId == id);
                if (hasRentals)
                    throw new InvalidOperationException("This motorcycle cannot be removed because it has associated rentals.");

                _context.Motos.Remove(moto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Finishing the Service RemoverMoto of MotoService... - {Data}", DateTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error when removing motorcycle - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when removing motorcycle: " + ex.Message);
            }
        }
    }
}
