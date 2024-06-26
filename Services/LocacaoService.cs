﻿using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Controllers;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services.RabbitMQ;
using Serilog;

namespace Rent_Motorcycle.Services
{
    public class LocacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocacaoService> _logger;
        private readonly RabbitMQSenderService _rabbitMQSender;

        public LocacaoService(ApplicationDbContext context, ILogger<LocacaoService> logger, RabbitMQSenderService rabbitMQConnection)
        {
            _context = context;
            _logger = logger;
            _rabbitMQSender = rabbitMQConnection;
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

                // Publica uma mensagem informando sobre a nova locação
                _logger.LogInformation("Publishing new rental message - RabbitMQ - {Data}", DateTime.Now);
                try
                {
                    if (_rabbitMQSender != null && _rabbitMQSender.IsConnected)
                    {
                        var message = JsonSerializer.Serialize(locacao);
                        await _rabbitMQSender.SendMessageAsync("new-rental", message);
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

                _logger.LogInformation("Finishing the Service EfetuarLocacao of LocacaoService - {Data}", DateTime.Now);
                return (null, valorTotal);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when executing the rental - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when making a Rental: " + ex.Message);
            }
        }

        public async Task<List<Locacao>> ConsultarLocacoes(DateTime? dataInicio,DateTime? dataPrevistaTermino,string? tipoPlanoNome,string? motoModelo, string? placaMoto, int? entregadorId,decimal? valorMinimo,decimal? valorMaximo)
        {
            try
            {
                _logger.LogInformation("Starting the Service ConsultarLocacoes of LocacaoService... - {Data}", DateTime.Now);

                var query = _context.Locacoes
                    .Include(l => l.TipoPlano)
                    .Include(l => l.Moto)
                    .Include(l => l.Entregador)
                    .AsQueryable();

                //Filtros de pesquisa
                if (dataInicio != null)
                    query = query.Where(l => l.DataInicio >= dataInicio);
                if (dataPrevistaTermino != null)
                    query = query.Where(l => l.DataPrevistaTermino <= dataPrevistaTermino);
                if (!string.IsNullOrEmpty(tipoPlanoNome))
                    query = query.Where(l => l.TipoPlano.Nome.Contains(tipoPlanoNome));
                if (!string.IsNullOrEmpty(motoModelo))
                    query = query.Where(l => l.Moto.Modelo.Contains(motoModelo));
                if (!string.IsNullOrEmpty(placaMoto))
                    query = query.Where(l => l.Moto.Placa.Contains(placaMoto));
                if (entregadorId != null)
                    query = query.Where(l => l.EntregadorId == entregadorId);
                if (valorMinimo != null)
                    query = query.Where(l => l.ValorLocacao >= valorMinimo);
                if (valorMaximo != null)
                    query = query.Where(l => l.ValorLocacao <= valorMaximo);

                var locacoes = await query.ToListAsync();

                _logger.LogInformation("Finishing the Service ConsultarLocacoes of LocacaoService... - {Data}", DateTime.Now);
                return locacoes;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when querying rentals - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                throw new Exception("Error when querying rentals: " + ex.Message);
            }
        }
    }
}
