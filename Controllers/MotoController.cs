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

        /// <summary>
        /// Endpoint to search for motorcycles by license plate.
        /// </summary>
        /// <param name="placa">License plate to filter the search.</param>
        /// <returns>List of motorcycles matching the search criteria.</returns>
        [HttpGet("search-motos")]
        public async Task<IActionResult> ConsultarMotos(string placa)
        {
            try
            {
                _logger.LogInformation("Starting the Controller ConsultarMotos of MotoController... - {Data}", DateTime.Now);
                var motos = await _motoService.ConsultarMotos(placa);
                _logger.LogInformation("Finishing the Controller ConsultarMotos of MotoController... - {Data}", DateTime.Now);
                return Ok(motos);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying motorcycles - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying motorcycles: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to retrieve all registered motorcycles.
        /// </summary>
        /// <returns>List of all registered motorcycles.</returns>
        [HttpGet("all-motos")]
        public async Task<IActionResult> ConsultarTodasAsMotos()
        {
            try
            {
                _logger.LogInformation("Starting the Controller ConsultarTodasAsMotos of MotoController... - {Data}", DateTime.Now);
                var motos = await _motoService.ConsultarMotos(null); 
                _logger.LogInformation("Finishing the Controller ConsultarTodasAsMotos of MotoController... - {Data}", DateTime.Now);
                return Ok(motos);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying all motorcycles - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying all motorcycles: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to modify a motorcycle by changing only its license plate.
        /// </summary>
        /// <param name="id">ID of the motorcycle to be modified.</param>
        /// <param name="novaPlaca">New license plate for the motorcycle.</param>
        /// <returns>ActionResult with the modification result.</returns>
        [HttpPut("modify-moto/{id}")]
        public async Task<IActionResult> ModificarMoto(int id, string novaPlaca)
        {
            try
            {
                _logger.LogInformation("Starting the Controller ModificarMoto of MotoController... - {Data}", DateTime.Now);
                var result = await _motoService.ModificarMoto(id, novaPlaca);
                _logger.LogInformation("Finishing the Controller ModificarMoto of MotoController... - {Data}", DateTime.Now);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("Error when modifying motorcycle - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when modifying motorcycle: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to remove a motorcycle if it has no associated rentals.
        /// </summary>
        /// <param name="id">ID of the motorcycle to be removed.</param>
        /// <returns>ActionResult with the removal result.</returns>
        [HttpDelete("remove-moto/{id}")]
        public async Task<IActionResult> RemoverMoto(int id)
        {
            try
            {
                _logger.LogInformation("Starting the Controller RemoverMoto of MotoController... - {Data}", DateTime.Now);
                var result = await _motoService.RemoverMoto(id);
                _logger.LogInformation("Finishing the Controller RemoverMoto of MotoController... - {Data}", DateTime.Now);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error("Error when removing motorcycle - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when removing motorcycle: {ex.Message}");
            }
        }
    }
}
