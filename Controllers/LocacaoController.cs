using Microsoft.AspNetCore.Mvc;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
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

        [HttpPost]
        public async Task<IActionResult> EfetuarLocacao(Locacao locacao)
        {
            if (await _locacaoService.EfetuarLocacao(locacao))
                return Ok(); // Retorna 200 OK se a locação for bem-sucedida

            return BadRequest(); // Retorna 400 Bad Request se a locação não puder ser efetuada
        }

        [HttpGet("{locacaoId}/valor")]
        public async Task<IActionResult> CalcularValorTotalLocacao(int locacaoId)
        {
            var valorTotal = await _locacaoService.CalcularValorTotalLocacao(locacaoId);

            if (valorTotal == 0)
                return NotFound(); // Retorna 404 Not Found se a locação não for encontrada

            return Ok(valorTotal); // Retorna o valor total calculado
        }
    }
}
