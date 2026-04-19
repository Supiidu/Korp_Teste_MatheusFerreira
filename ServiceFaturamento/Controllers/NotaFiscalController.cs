using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFaturamento.Data;
using ServiceFaturamento.DTOs;
using ServiceFaturamento.Models;

namespace ServiceFaturamento.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotaFiscalController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;
        public NotaFiscalController(AppDbContext appDbContext, HttpClient httpClient)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> CriarNota()
        {
            var ultimoNumero = await _appDbContext.Notas
            .OrderByDescending(n => n.Numero)
            .Select(n => n.Numero)
            .FirstOrDefaultAsync();

            var nota = new Nota
            {
                Numero = ultimoNumero + 1,
                DataEmissao = DateTime.UtcNow,
                Status = Enums.StatusNota.Aberta
            };
            _appDbContext.Notas.Add(nota);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNota), new { id = nota.Id }, nota);
        }

        [HttpPost("{id}/itens")]
        public async Task<IActionResult> AdicionarItem(int id, ItemNotaDTO notaDTO)
        {
            var nota = await
                _appDbContext.Notas
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
            {
                return NotFound();
            }
            if (nota.Status != Enums.StatusNota.Aberta)
            {
                return BadRequest("Não é possível adicionar itens a uma nota fechada.");
            }
            if (string.IsNullOrEmpty(notaDTO.CodigoProduto))
            {
                return BadRequest("Código do produto é obrigatório.");
            }
            if (notaDTO.Quantidade <= 0)
            {
                return BadRequest("Quantidade deve ser maior que zero.");
            }

            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:5226/api/Estoque/codigo/{notaDTO.CodigoProduto}");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Produto não encontrado no estoque.");
                }
            }
            catch
            {
                return StatusCode(503, "Erro ao verificar o produto no estoque.");
            }

            var itemNota = new ItemNota
            {
                NotaFiscalId = id,
                CodigoProduto = notaDTO.CodigoProduto,
                Quantidade = notaDTO.Quantidade
            };
            nota.Itens.Add(itemNota);
            await _appDbContext.SaveChangesAsync();
            return Ok(itemNota);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNota(int id)
        {
            var nota = await _appDbContext.Notas
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
                return NotFound();

            return Ok(nota);
        }
        [HttpPost("{id}/fechar")]
        public async Task<IActionResult> FecharNota(int id)
        {
            var nota = await
                _appDbContext.Notas
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
            {
                return NotFound();
            }
            if (nota.Status != Enums.StatusNota.Aberta)
            {
                return BadRequest("A nota já está fechada.");
            }
            if (nota.Itens.Count == 0)
            {
                return BadRequest("Não é possível fechar uma nota sem itens.");
            }
            foreach (var item in nota.Itens)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(
                    "http://localhost:5226/api/estoque/baixar",
                    new
                    {
                        codigo = item.CodigoProduto,
                        quantidade = item.Quantidade
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        var mensagem = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, mensagem);
                    }
                }
                catch
                {
                    return StatusCode(503, "Erro ao verificar o produto no estoque.");
                }
            }
            nota.Status = Enums.StatusNota.Fechada;
            await _appDbContext.SaveChangesAsync();
            return Ok(" Nota fechada com sucesso.");

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nota>>> GetNotas()
        {
            var lista = await _appDbContext.Notas
                .Include(n => n.Itens)
                .ToListAsync();
            return Ok(lista);
        }
        [HttpDelete("{id}/itens/{itemId}")]
        public async Task<IActionResult> RemoverItem(int id, int itemId)
        {
            var nota = await _appDbContext.Notas
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
                return NotFound("Nota não encontrada.");

            if (nota.Status != Enums.StatusNota.Aberta)
                return BadRequest("Não é possível remover itens de uma nota fechada.");

            var item = nota.Itens.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                return NotFound("Item não encontrado.");

            _appDbContext.Remove(item);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}