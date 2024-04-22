using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using Rent_Motorcycle.Utils;
using Serilog;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Rent_Motorcycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotoController : ControllerBase
    {
        private readonly MotoService _motoService;
        private readonly ILogger<MotoController> _logger;

        public MotoController(MotoService motoService, ILogger<MotoController> logger)
        {
            _motoService = motoService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to register a motorcycle.
        /// </summary>
        /// <param name="moto">Motorcycle details to be registered.</param>
        /// <returns>ActionResult with the registration result.</returns>
        [HttpPost("register-motorcycle")]
        public async Task<IActionResult> CadastrarMoto(Moto moto)
        {
            try
            {
                _logger.LogInformation("Starting the Controller CadastrarMoto of MotoController... - {Data}", DateTime.Now);
                await _motoService.CadastrarMoto(moto);
                _logger.LogInformation("Finishing the Controller CadastrarMoto of MotoController... - {Data}", DateTime.Now);
                return Ok("Motorcycle registered successfully.");
            }
            catch (InvalidOperationException ex)
            {
                Log.Error("Error when registering Motorcycle - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when registering Motorcycle: {ex.Message}");
            }
        }
    }
}
