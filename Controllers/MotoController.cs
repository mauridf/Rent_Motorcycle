using Microsoft.AspNetCore.Mvc;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using System.Threading.Tasks;

namespace Rent_Motorcycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotoController : ControllerBase
    {
        private readonly MotoService _motoService;

        public MotoController(MotoService motoService)
        {
            _motoService = motoService;
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarMoto(Moto moto)
        {
            try
            {
                await _motoService.CadastrarMoto(moto);
                return Ok("Moto cadastrada com sucesso.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
