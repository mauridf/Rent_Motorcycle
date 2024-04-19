using Microsoft.AspNetCore.Mvc;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using System.Threading.Tasks;

namespace Rent_Motorcycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntregadorController : ControllerBase
    {
        private readonly EntregadorService _entregadorService;

        public EntregadorController(EntregadorService entregadorService)
        {
            _entregadorService = entregadorService;
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarEntregador(Entregador entregador)
        {
            try
            {
                await _entregadorService.CadastrarEntregador(entregador);
                return Ok("Entregador cadastrado com sucesso.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
