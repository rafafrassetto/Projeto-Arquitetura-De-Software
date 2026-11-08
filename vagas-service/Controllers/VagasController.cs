using Microsoft.AspNetCore.Mvc;
using VagasService.DTO;
using VagasService.Servicos;

namespace VagasService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("[controller]")]
    public class VagasController : ControllerBase
    {
        private readonly ServVagas _servVagas;

        public VagasController()
        {
            _servVagas = new ServVagas();
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] string? status, [FromQuery] string? setor)
        {
            try
            {
                var vagas = _servVagas.Listar(status, setor);
                return Ok(vagas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                var vaga = await _servVagas.ObterPorId(id);
                if (vaga == null)
                    return NotFound(new { mensagem = $"Vaga com ID {id} não encontrada." });

                return Ok(vaga);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Criar([FromBody] CriarVagaDTO dto)
        {
            try
            {
                var novaVaga = _servVagas.Criar(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = novaVaga.Id }, novaVaga);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] AtualizarVagaDTO dto)
        {
            try
            {
                var atualizada = _servVagas.Atualizar(id, dto);
                return Ok(atualizada);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPatch("{id}/alocar")]
        public IActionResult Alocar(int id, [FromBody] AlocarVagaDTO dto)
        {
            try
            {
                var vaga = _servVagas.Alocar(id, dto.VeiculoId);
                return Ok(vaga);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPatch("{id}/liberar")]
        [HttpPut("{id}/liberar")]
        public IActionResult Liberar(int id)
        {
            try
            {
                var vaga = _servVagas.Liberar(id);
                return Ok(new { mensagem = $"Vaga {vaga.Codigo} liberada com sucesso.", vaga });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            try
            {
                _servVagas.Deletar(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("sugestao-ia")]
        public IActionResult SugerirVagaIa([FromQuery] string? tipoVeiculo)
        {
            try
            {
                var sugestao = _servVagas.SugerirVagaIa(tipoVeiculo);
                return Ok(sugestao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                servico = "vagas-service",
                status = "Healthy",
                horario = DateTime.UtcNow
            });
        }
    }
}
