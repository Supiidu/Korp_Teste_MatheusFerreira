using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceEstoque.Data;
using ServiceEstoque.DTOS;
using ServiceEstoque.Models;

namespace ServiceEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public EstoqueController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduto(Produto produto)
        {
            if (produto.Saldo < 0)
            {
                return BadRequest("O saldo do produto não pode ser negativo.");
            }
            _appDbContext.Estoque.Add(produto);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var lista = await _appDbContext.Estoque.ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _appDbContext.Estoque.FindAsync(id);

            if (produto == null)
                return NotFound();

            return produto;
        }
        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<Produto>> GetByCodigo(string codigo)
        {
            var produto = await _appDbContext.Estoque
            .FirstOrDefaultAsync(p => p.Codigo == codigo);

            if (produto == null)
                return NotFound();

            return produto;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduto(int id, [FromBody] Produto produto)
        {
            var produtoExistente = await _appDbContext.Estoque.FindAsync(id);
            if (produtoExistente == null)
                return NotFound();
            _appDbContext.Entry(produtoExistente).CurrentValues.SetValues(produto);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _appDbContext.Estoque.FindAsync(id);
            if (produto == null)
                return NotFound();

            _appDbContext.Estoque.Remove(produto);

            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("baixar")]
        public async Task<IActionResult> BaixarEstoque([FromBody] BaixaEstoqueDto dto)
        {
            var produto = await _appDbContext.Estoque
                .FirstOrDefaultAsync(p => p.Codigo == dto.Codigo);

            if (produto == null)
                return NotFound($"Produto {dto.Codigo} não encontrado");

            if (produto.Saldo < dto.Quantidade)
                return BadRequest("Saldo insuficiente em estoque");

            produto.Saldo -= dto.Quantidade;

            await _appDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}