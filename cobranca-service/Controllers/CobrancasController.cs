using CobrancaService.DTO;
using CobrancaService.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CobrancaService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("[controller]")]
    public class CobrancasController : ControllerBase
    {
        private readonly ServCobrancas _servCobrancas;

        public CobrancasController()
        {
            _servCobrancas = new ServCobrancas();
        }

        [HttpGet]
        public IActionResult Listar(
            [FromQuery] string? statusPagamento,
            [FromQuery] int? veiculoId,
            [FromQuery] int? vagaId,
            [FromQuery] int? operadorId)
        {
            try
            {
                var cobrancas = _servCobrancas.Listar(statusPagamento, veiculoId, vagaId, operadorId);
                return Ok(cobrancas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult ObterPorId(int id)
        {
            try
            {
                var cobranca = _servCobrancas.ObterPorId(id);
                if (cobranca == null)
                    return NotFound(new { mensagem = $"Cobrança com ID {id} não encontrada." });

                return Ok(cobranca);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("{id:int}/calcular")]
        public IActionResult CalcularTarifa(int id)
        {
            try
            {
                var simulacao = _servCobrancas.CalcularTarifa(id);
                return Ok(simulacao);
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

        [HttpPost]
        public IActionResult Criar([FromBody] CriarCobrancaDTO dto)
        {
            try
            {
                var novaCobranca = _servCobrancas.Criar(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = novaCobranca.Id }, novaCobranca);
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

        [HttpPut("{id:int}")]
        public IActionResult Atualizar(int id, [FromBody] AtualizarCobrancaDTO dto)
        {
            try
            {
                var atualizada = _servCobrancas.Atualizar(id, dto);
                return Ok(atualizada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
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

        [HttpPatch("{id:int}/pagar")]
        [HttpPut("{id:int}/pagar")]
        public IActionResult ConfirmarPagamento(
            int id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ConfirmarPagamentoDTO? dto = null)
        {
            try
            {
                var cobranca = _servCobrancas.ConfirmarPagamento(id, dto);
                return Ok(new { mensagem = $"Pagamento da cobrança {cobranca.Id} confirmado com sucesso.", cobranca });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
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

        [HttpPatch("{id:int}/cancelar")]
        public IActionResult Cancelar(int id)
        {
            try
            {
                var cobranca = _servCobrancas.Cancelar(id);
                return Ok(new { mensagem = $"Cobrança {cobranca.Id} cancelada com sucesso.", cobranca });
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

        [HttpDelete("{id:int}")]
        public IActionResult Deletar(int id)
        {
            try
            {
                _servCobrancas.Deletar(id);
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

        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                servico = "cobranca-service",
                status = "Healthy",
                horario = DateTime.UtcNow
            });
        }
    }
}
