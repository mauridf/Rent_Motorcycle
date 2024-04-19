using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;

namespace Rent_Motorcycle.Services
{
    public class LocacaoService
    {
        private readonly ApplicationDbContext _context;

        public LocacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalcularValorTotalLocacao(int locacaoId)
        {
            var locacao = await _context.Locacoes
                .Include(l => l.TipoPlano)
                .FirstOrDefaultAsync(l => l.Id == locacaoId);

            if (locacao == null)
                return 0; // Retorna 0 se a locação não for encontrada

            // Cálculo do valor total da locação de acordo com o tipo de plano
            decimal valorTotal = locacao.TipoPlano.Custo;

            // Implemente aqui a lógica de cálculo com base nas informações fornecidas
            // Exemplo: valorTotal += lógica de cálculo

            return valorTotal;
        }

        public async Task<bool> EfetuarLocacao(Locacao locacao)
        {
            // Verifica se o entregador está habilitado na categoria A
            var entregador = await _context.Entregadores
                .FirstOrDefaultAsync(e => e.Id == locacao.EntregadorId);

            if (entregador == null || entregador.TipoCNH != "A")
                return false; // Retorna false se o entregador não estiver habilitado na categoria A

            // Salva a locação no banco de dados
            _context.Locacoes.Add(locacao);
            await _context.SaveChangesAsync();

            return true; // Retorna true se a locação for bem-sucedida
        }
    }
}