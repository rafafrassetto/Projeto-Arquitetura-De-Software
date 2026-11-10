using Microsoft.AspNetCore.Mvc;
using VeiculosService.DTO;
using VeiculosService.Servicos;

namespace VeiculosService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("[controller]")]
    public class VeiculosController : ControllerBase
    {
        private readonly ServVeiculos _servVeiculos;

        public VeiculosController()
        {
            _servVeiculos = new ServVeiculos();
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] bool? ativoNoPatio, [FromQuery] string? placa)
        {
            try
            {
                var veiculos = _servVeiculos.Listar(ativoNoPatio, placa);
                return Ok(veiculos);
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
                var veiculo = _servVeiculos.ObterPorId(id);
                if (veiculo == null)
                    return NotFound(new { mensagem = $"Veículo com ID {id} não encontrado." });

                return Ok(veiculo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpGet("placa/{placa}")]
        public IActionResult ObterPorPlaca(string placa)
        {
            try
            {
                var veiculo = _servVeiculos.ObterPorPlaca(placa);
                if (veiculo == null)
                    return NotFound(new { mensagem = $"Veículo com placa '{placa}' não encontrado." });

                return Ok(veiculo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Criar([FromBody] CriarVeiculoDTO dto)
        {
            try
            {
                var novoVeiculo = _servVeiculos.Criar(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = novoVeiculo.Id }, novoVeiculo);
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
        public IActionResult Atualizar(int id, [FromBody] AtualizarVeiculoDTO dto)
        {
            try
            {
                var atualizado = _servVeiculos.Atualizar(id, dto);
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

        [HttpPatch("{id:int}/entrada")]
        public IActionResult RegistrarEntrada(int id)
        {
            try
            {
                var veiculo = _servVeiculos.RegistrarEntrada(id);
                return Ok(new { mensagem = $"Entrada do veículo {veiculo.Placa} registrada no pátio.", veiculo });
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

        [HttpPatch("{id:int}/saida")]
        public IActionResult RegistrarSaida(int id)
        {
            try
            {
                var veiculo = _servVeiculos.RegistrarSaida(id);
                return Ok(new { mensagem = $"Saída do veículo {veiculo.Placa} registrada do pátio.", veiculo });
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

        [HttpDelete("{id:int}")]
        public IActionResult Deletar(int id)
        {
            try
            {
                _servVeiculos.Deletar(id);
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
                servico = "veiculos-service",
                status = "Healthy",
                horario = DateTime.UtcNow
            });
        }
    }
}
