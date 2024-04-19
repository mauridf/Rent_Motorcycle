using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;

namespace Rent_Motorcycle.Services
{
    public class EntregadorService
    {
        private readonly ApplicationDbContext _context;

        public EntregadorService(ApplicationDbContext context)
        {
            _context = context;
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
}
