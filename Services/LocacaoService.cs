using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Controllers;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Serilog;

namespace Rent_Motorcycle.Services
{
    public class LocacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocacaoService> _logger;

        public LocacaoService(ApplicationDbContext context, ILogger<LocacaoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public decimal CalcularValorTotalLocacao(DateTime dataInicio, int tipoPlanoId, DateTime dataTermino)
        {
            try
            {
                _logger.LogInformation("Starting the Service CalcularValorTotalLocacao of LocacaoService... - {Data}", DateTime.Now);
                var tipoPlano = _context.TipoPlanos.FirstOrDefault(tp => tp.Id == tipoPlanoId);
                if (tipoPlano == null)
                {
                    return 0;
                }

                // Calcula a data prevista de término com base no tipo de plano e na data de início
                var dataPrevistaTermino = dataInicio.AddDays(tipoPlano.Dias);

                // Calcula o valor total da locação de acordo com as novas regras
                decimal valorTotal = tipoPlano.Custo; // Valor base é o custo do tipo de plano

                if (dataTermino > dataPrevistaTermino)
                {
                    TimeSpan diferencaDias = dataTermino - dataPrevistaTermino;
                    valorTotal += diferencaDias.Days * 50; // Adiciona R$50,00 por diária adicional
                }
                else if (dataTermino < dataPrevistaTermino)
                {
                    // Calcula o número de dias de antecipação em relação à data prevista de término
                    TimeSpan diferencaDias = dataPrevistaTermino - dataTermino;
                    decimal multa = 0;

                    // Calcula a multa de acordo com o tipo de plano
                    switch (tipoPlanoId)
                    {
                        case 1: // Plano de 7 dias
                            multa = tipoPlano.Custo * 0.2m; // 20% sobre o valor do plano
                            break;
                        case 2: // Plano de 15 dias
                            multa = tipoPlano.Custo * 0.4m; // 40% sobre o valor do plano
                            break;
                        default:
                            // Não há multa para outros tipos de plano
                            break;
                    }

                    valorTotal += multa;
                }
                _logger.LogInformation("Valor {valor} calculated - {Data}", valorTotal, DateTime.Now);
                _logger.LogInformation("Finishing the Service CalcularValorTotalLocacao of LocacaoService... - {Data}", DateTime.Now);
                return valorTotal;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred in the Rental Value Calculation - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when calculating rental value: " + ex.Message);
            }
        }

        public async Task<(string, decimal)> EfetuarLocacao(Locacao locacao)
        {
            try
            {
                _logger.LogInformation("Starting the Service EfetuarLocacao of LocacaoService... - {Data}", DateTime.Now);
                // Busca o TipoPlano correspondente do banco de dados
                var tipoPlano = await _context.TipoPlanos.FirstOrDefaultAsync(tp => tp.Id == locacao.TipoPlanoId);
                // Verifica se o entregador está habilitado na categoria A
                var entregador = await _context.Entregadores.FirstOrDefaultAsync(e => e.Id == locacao.EntregadorId && e.TipoCNH == "A");
                if (entregador == null)
                    return ("Only Deliverers with Type A License can carry out Rentals.", 0);

                // Verifica se a data de início é válida (deve ser o próximo dia após a data de criação)
                var dataCriacao = DateTime.Today.AddDays(1); // Próximo dia após a data de criação
                if (locacao.DataInicio.Date != dataCriacao)
                {
                    locacao.DataInicio = dataCriacao; // Define a data de início como o próximo dia após a data de criação
                    locacao.DataPrevistaTermino = locacao.DataInicio.AddDays(tipoPlano.Dias); // Atualiza a data prevista de término
                }

                // Verifica se o TipoPlano existe
                if (tipoPlano == null)
                    return ("Invalid Plan Type.", 0);

                // Calcula a data prevista de término com base no tipo de plano
                locacao.DataPrevistaTermino = locacao.DataInicio.AddDays(tipoPlano.Dias);

                // Calcula o valor total da locação
                var valorTotal = CalcularValorTotalLocacao(locacao.DataInicio, locacao.TipoPlanoId, locacao.DataTermino);

                // Atribui o valor total calculado à propriedade ValorLocacao
                locacao.ValorLocacao = valorTotal;

                // Salva a locação no banco de dados
                _context.Locacoes.Add(locacao);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Finishing the Service EfetuarLocacao of LocacaoService - {Data}", DateTime.Now);
                return (null, valorTotal); // Retorna null para a mensagem de erro e o valor total calculado
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when executing the rental - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when making a Rental: " + ex.Message);
            }
        }
    }
}
