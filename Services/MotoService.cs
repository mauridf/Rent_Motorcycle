using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;

namespace Rent_Motorcycle.Services
{
    public class MotoService
    {
        private readonly ApplicationDbContext _context;

        public MotoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CadastrarMoto(Moto moto)
        {
            // Verifica se a placa já está cadastrada
            if (await _context.Motos.AnyAsync(m => m.Placa == moto.Placa))
                throw new InvalidOperationException("Essa Placa já foi cadastrada.");

            // Salva a moto no banco de dados
            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            return true; // Retorna true se o cadastro for bem-sucedido
        }
    }
}
