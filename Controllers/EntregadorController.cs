using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rent_Motorcycle.Models;
using Rent_Motorcycle.Services;
using Rent_Motorcycle.Utils;
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

        public EntregadorController(EntregadorService entregadorService)
        {
            _entregadorService = entregadorService;
        }

        /// <summary>
        /// Endpoint para cadastrar um entregador.
        /// </summary>
        /// <param name="entregador">Dados do entregador a ser cadastrado.</param>
        /// <returns>ActionResult com o resultado do cadastro.</returns>
        [HttpPost("cadastrar-entregador")]
        public async Task<IActionResult> CadastrarEntregador([FromBody] Entregador entregador)
        {
            try
            {
                await _entregadorService.CadastrarEntregador(entregador);
                return Ok("Entregador cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar entregador: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint para upload de imagem.
        /// </summary>
        /// <param name="imagem">Imagem a ser enviada.</param>
        /// <returns>URL da imagem após o upload.</returns>
        [HttpPost("upload-imagem")]
        [FileUpload]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImagem([FromForm] IFormFile imagem)
        {
            if (imagem == null || imagem.Length == 0)
            {
                return BadRequest("Nenhuma imagem foi enviada.");
            }

            try
            {
                byte[] imagemBytes;
                string nomeArquivo;

                using (var memoryStream = new MemoryStream())
                {
                    await imagem.CopyToAsync(memoryStream);
                    imagemBytes = memoryStream.ToArray();
                }

                // Verifica se o tipo de arquivo é suportado (PNG ou BMP)
                if (imagem.ContentType != "image/png" && imagem.ContentType != "image/bmp")
                {
                    return BadRequest("Somente arquivos PNG ou BMP são suportados.");
                }

                // Tenta obter o nome do arquivo a partir do Content-Disposition
                var contentDisposition = ContentDispositionHeaderValue.Parse(imagem.ContentDisposition);
                nomeArquivo = contentDisposition.FileName.ToString().Trim('"');

                nomeArquivo = $"{Guid.NewGuid()}.png"; //Alterar o Nome do Arquivo para ficar padronizado

                // Faz o upload da imagem
                var imagemUrl = await _entregadorService.UploadImagem(imagemBytes, nomeArquivo);
                return Ok(new { ImagemUrl = imagemUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao fazer o upload da imagem: {ex.Message}");
            }
        }
    }
}