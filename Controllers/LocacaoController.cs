using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_Motorcycle.Data;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using System;
using System.Threading.Tasks;

namespace Rent_Motorcycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocacaoController : ControllerBase
    {
        private readonly LocacaoService _locacaoService;

        public LocacaoController(LocacaoService locacaoService)
        {
            _locacaoService = locacaoService;
        }

        [HttpPost("efetuar")]
        public async Task<IActionResult> EfetuarLocacao(Locacao locacao)
        {
            var (mensagemErro, valorTotal) = await _locacaoService.EfetuarLocacao(locacao);

            if (mensagemErro != null)
                return BadRequest(mensagemErro); // Retorna 400 Bad Request com a mensagem de erro se a locação não puder ser efetuada

            return Ok(valorTotal); // Retorna 200 OK e o valor total calculado se a locação for bem-sucedida
        }


        [HttpGet("calcular-valor")]
        public IActionResult CalcularValorTotalLocacao([FromQuery] DateTime dataInicio, int tipoPlano, [FromQuery] DateTime dataTermino)
        {
            // Verifica se a data de término é válida (após a data de início)
            if (dataTermino.Date <= dataInicio.Date)
                return BadRequest("Data de término deve ser posterior à data de início.");

            //Levando em consideração a regra que diz que a Data de Início é um dia após o registro da Locação,
            //vou acrescentar um dia a mais na dataInicio que o Entregador entrar para fazer a pesquisa,
            //pois se ele depois for efetivar a locação em seguida de fazer a pesquisa pode dar diferença no valor.
            dataInicio = dataInicio.AddDays(1);

            // Cálculo do valor total da locação com base nos parâmetros fornecidos
            var valorTotal = _locacaoService.CalcularValorTotalLocacao(dataInicio, tipoPlano, dataTermino);

            return Ok(valorTotal); // Retorna o valor total calculado
        }
    }
}