﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Rent_Motorcycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocacaoController : ControllerBase
    {
        private readonly LocacaoService _locacaoService;
        private readonly ILogger<LocacaoController> _logger;

        public LocacaoController(LocacaoService locacaoService, ILogger<LocacaoController> logger)
        {
            _locacaoService = locacaoService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to make a rental.
        /// </summary>
        /// <param name="locacao">Delivery person details to be registered.</param>
        /// <returns>ActionResult with the registration result.</returns>
        [HttpPost("make-rental")]
        public async Task<IActionResult> EfetuarLocacao(Locacao locacao)
        {
            try
            {
                _logger.LogInformation("Starting the Controller EfetuarLocacao of LocacaoController... - {Data}", DateTime.Now);
                var (mensagemErro, valorTotal) = await _locacaoService.EfetuarLocacao(locacao);

                if (mensagemErro != null)
                    return BadRequest(mensagemErro); // Retorna 400 Bad Request com a mensagem de erro se a locação não puder ser efetuada

                _logger.LogInformation("Finishing the Controller EfetuarLocacao of LocacaoController... - {Data}", DateTime.Now);
                return Ok(valorTotal); // Retorna 200 OK e o valor total calculado se a locação for bem-sucedida
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when executing the rental - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when making rental: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to calculate a rental value.
        /// </summary>
        /// <param name="dataInicio">Start Date.</param>
        /// <param name="tipoPlano">The Type of Plan chosen.</param>
        /// <param name="dataTermino">End Date.</param>
        /// <returns>ActionResult with the final calculated value.</returns>
        [HttpGet("calculate-rental-value")]
        public IActionResult CalcularValorTotalLocacao([FromQuery] DateTime dataInicio, int tipoPlano, [FromQuery] DateTime dataTermino)
        {
            try
            {
                _logger.LogInformation("Starting the Controller CalcularValorTotalLocacao of LocacaoController... - {Data}", DateTime.Now);
                // Verifica se a data de término é válida (após a data de início)
                if (dataTermino.Date <= dataInicio.Date)
                    return BadRequest("Data de término deve ser posterior à data de início.");

                //Levando em consideração a regra que diz que a Data de Início é um dia após o registro da Locação,
                //vou acrescentar um dia a mais na dataInicio que o Entregador entrar para fazer a pesquisa,
                //pois se ele depois for efetivar a locação em seguida de fazer a pesquisa pode dar diferença no valor.
                dataInicio = dataInicio.AddDays(1);

                // Cálculo do valor total da locação com base nos parâmetros fornecidos
                var valorTotal = _locacaoService.CalcularValorTotalLocacao(dataInicio, tipoPlano, dataTermino);

                _logger.LogInformation("Finishing the Controller CalcularValorTotalLocacao of LocacaoController... - {Data}", DateTime.Now);
                return Ok(valorTotal); // Retorna o valor total calculado
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred in the Rental Value Calculation - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when making rental: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to retrieve all rentals.
        /// </summary>
        /// <returns>List of all rentals.</returns>
        [HttpGet("all-rentals")]
        public async Task<IActionResult> GetLocacoes()
        {
            try
            {
                _logger.LogInformation("Starting the Controller GetLocacoes of LocacaoController... - {Data}", DateTime.Now);
                var locacoes = await _locacaoService.ConsultarLocacoes(null, null, null, null, null, null, null, null);
                _logger.LogInformation("Finishing the Controller GetLocacoes of LocacaoController... - {Data}", DateTime.Now);
                return Ok(locacoes);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying rentals - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying rentals: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to search rentals by filters.
        /// </summary>
        /// <param name="dataInicio">Start date of the rental period.</param>
        /// <param name="dataPrevistaTermino">Expected end date of the rental period.</param>
        /// <param name="tipoPlanoNome">Name of the rental plan type.</param>
        /// <param name="motoModelo">Model of the motorcycle.</param>
        /// <param name="motoPlaca">license plate</param>
        /// <param name="entregadorId">ID of the delivery person.</param>
        /// <param name="valorMinimo">Minimum rental value.</param>
        /// <param name="valorMaximo">Maximum rental value.</param>
        /// <returns>List of rentals matching the specified filters.</returns>
        [HttpGet("search-rental")]
        public async Task<IActionResult> PesquisarLocacoes(
            DateTime? dataInicio,
            DateTime? dataPrevistaTermino,
            string? tipoPlanoNome,
            string? motoModelo,
            string? motoPlaca,
            int? entregadorId,
            decimal? valorMinimo,
            decimal? valorMaximo)
        {
            try
            {
                _logger.LogInformation("Starting the Controller PesquisarLocacoes of LocacaoController... - {Data}", DateTime.Now);
                var locacoes = await _locacaoService.ConsultarLocacoes(dataInicio, dataPrevistaTermino, tipoPlanoNome, motoModelo, motoPlaca, entregadorId, valorMinimo, valorMaximo);
                _logger.LogInformation("Finishing the Controller PesquisarLocacoes of LocacaoController... - {Data}", DateTime.Now);
                return Ok(locacoes);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying rentals by filters - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying rentals by filters: {ex.Message}");
            }
        }
    }
}