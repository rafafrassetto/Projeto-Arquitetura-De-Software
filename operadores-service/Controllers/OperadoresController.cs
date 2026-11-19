using Microsoft.AspNetCore.Mvc;
using OperadoresService.DTO;
using OperadoresService.Servicos;

namespace OperadoresService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("[controller]")]
    public class OperadoresController : ControllerBase
    {
        private readonly ServOperadores _servOperadores;

        public OperadoresController()
        {
            _servOperadores = new ServOperadores();
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] string? turno, [FromQuery] string? cargo, [FromQuery] bool? ativo)
        {
            try
            {
                var operadores = _servOperadores.Listar(turno, cargo, ativo);
                return Ok(operadores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObterPorId(int id)
        {
            try
            {
                var operador = _servOperadores.ObterPorId(id);
                if (operador == null)
                    return NotFound(new { mensagem = $"Operador com ID {id} não encontrado." });

                return Ok(operador);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("matricula/{matricula}")]
        public IActionResult ObterPorMatricula(string matricula)
        {
            try
            {
                var operador = _servOperadores.ObterPorMatricula(matricula);
                if (operador == null)
                    return NotFound(new { mensagem = $"Operador com matrícula '{matricula}' não encontrado." });

                return Ok(operador);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Criar([FromBody] CriarOperadorDTO dto)
        {
            try
            {
                var novoOperador = _servOperadores.Criar(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = novoOperador.Id }, novoOperador);
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
        public IActionResult Atualizar(int id, [FromBody] AtualizarOperadorDTO dto)
        {
            try
            {
                var atualizado = _servOperadores.Atualizar(id, dto);
                return Ok(atualizado);
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

        [HttpPatch("{id}/status")]
        public IActionResult AlternarStatus(int id, [FromBody] AlterarStatusDTO dto)
        {
            try
            {
                var operador = _servOperadores.AlternarStatus(id, dto.Ativo);
                return Ok(new { mensagem = $"Status do operador atualizado para '{(dto.Ativo ? "Ativo" : "Inativo")}'.", operador });
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
                _servOperadores.Deletar(id);
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
    }
}
