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
    public class EntregadorController : ControllerBase
    {
        private readonly EntregadorService _entregadorService;
        private readonly ILogger<EntregadorController> _logger;

        public EntregadorController(EntregadorService entregadorService, ILogger<EntregadorController> logger)
        {
            _entregadorService = entregadorService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to register a delivery person.
        /// </summary>
        /// <param name="entregador">Delivery person details to be registered.</param>
        /// <returns>ActionResult with the registration result.</returns>
        [HttpPost("register-delivery")]
        public async Task<IActionResult> CadastrarEntregador([FromBody] Entregador entregador)
        {
            try
            {
                _logger.LogInformation("Starting the Controller CadastrarEntregador of EntregadorController... - {Data}", DateTime.Now);
                await _entregadorService.CadastrarEntregador(entregador);
                _logger.LogInformation("Finishing the Controller CadastrarEntregador of EntregadorController... - {Data}", DateTime.Now);
                return Ok("Entregador cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                Log.Error("An error when registering delivery driver - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when registering delivery driver: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to retrieve all delivery drivers.
        /// </summary>
        /// <returns>List of all delivery drivers.</returns>
        [HttpGet("all-delivery-drivers")]
        public async Task<IActionResult> GetEntregadores()
        {
            try
            {
                _logger.LogInformation("Starting the Controller GetEntregadores of EntregadorController... - {Data}", DateTime.Now);
                var entregadores = await _entregadorService.ConsultarEntregadores(null, null, null, null, null);
                _logger.LogInformation("Finishing the Controller GetEntregadores of EntregadorController... - {Data}", DateTime.Now);
                return Ok(entregadores);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying delivery driver - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying delivery driver: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint to search delivery driver by filters.
        /// </summary>
        /// <param name="dataNascimento">Start date of the rental period.</param>
        /// <param name="nome">Expected end date of the rental period.</param>
        /// <param name="cnpj">Name of the rental plan type.</param>
        /// <param name="cnh">Model of the motorcycle.</param>
        /// <param name="tipoCNH">license plate</param>
        /// <returns>List of delivery driver matching the specified filters.</returns>
        [HttpGet("search-delivery-driver")]
        public async Task<IActionResult> PesquisarEntregador(
            DateTime? dataNascimento,
            string? nome,
            string? cnpj,
            string? cnh,
            string? tipoCNH)
        {
            try
            {
                _logger.LogInformation("Starting the Controller GetEntregadores of EntregadorController... - {Data}", DateTime.Now);
                var locacoes = await _entregadorService.ConsultarEntregadores(dataNascimento, nome, cnpj, cnh, tipoCNH);
                _logger.LogInformation("Finishing the Controller GetEntregadores of EntregadorController... - {Data}", DateTime.Now);
                return Ok(locacoes);
            }
            catch (Exception ex)
            {
                Log.Error("Error when querying delivery driver by filters - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error when querying delivery driver by filters: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint for image upload.
        /// </summary>
        /// <param name="imagem">Image to be sent.</param>
        /// <returns>Image URL after upload.</returns>
        [HttpPost("upload-image")]
        [FileUpload]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImagem([FromForm] IFormFile imagem)
        {
            if (imagem == null || imagem.Length == 0)
            {
                return BadRequest("No images were sent.");
            }

            try
            {
                _logger.LogInformation("Starting the Controller UploadImagem of EntregadorController... - {Data}", DateTime.Now);
                byte[] imagemBytes;
                string nomeArquivo;

                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    imagemBytes = memoryStream.ToArray();
                }

                // Tenta obter o nome do arquivo a partir do Content-Disposition
                var contentDisposition = ContentDispositionHeaderValue.Parse(imagem.ContentDisposition);
                var nomeArquivoOriginal = contentDisposition.FileName.ToString().Trim('"');

                // Obtém a extensão do arquivo original
                var extensaoOriginal = Path.GetExtension(nomeArquivoOriginal)?.ToLower();

                // Verifica se a extensão é suportada (PNG ou BMP)
                if (extensaoOriginal != ".png" && extensaoOriginal != ".bmp")
                {
                    return BadRequest("Only PNG or BMP files are supported.");
                }

                // Gera um novo nome de arquivo com a extensão corrigida
                nomeArquivo = $"{Guid.NewGuid()}{extensaoOriginal}";

                // Faz o upload da imagem
                var imagemUrl = await _entregadorService.UploadImagem(imagemBytes, nomeArquivo);
                _logger.LogInformation("Finishing the Controller UploadImagem of EntregadorController... - {Data}", DateTime.Now);
                return Ok(new { ImagemUrl = imagemUrl });
            }
            catch (Exception ex)
            {
                Log.Error("Error uploading image - {MinhaMsgErro}. Data: {MinhaData}", ex.Message, DateTime.Now);
                return StatusCode(500, $"Error uploading image: {ex.Message}");
            }
        }
    }
}