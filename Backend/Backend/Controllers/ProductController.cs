using EmployeeCrudApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Backend.Repositories;

namespace EmployeeCrudApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository repo, ILogger<ProductController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<Product>> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return list;
        }

        [HttpGet("{id}")]
        public async Task<Product> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid) { _logger.LogWarning("Invalid model state on Create"); return BadRequest(ModelState); }
            if (product.Stock < 0 || product.Stock > 100) return BadRequest(new { error = "Stock must be between 0 and 100" });

            if (string.IsNullOrWhiteSpace(product.Name)) return BadRequest(new { error = "Name is required" });

            // Validate name: no digits
            if (product.Name.Any(char.IsDigit)) return BadRequest(new { error = "Name must not contain digits" });

            // Reject excessive repeated characters (more than 3 repeats)
            bool HasExcessiveRepeats(string s)
            {
                int count = 1;
                for (int i = 1; i < s.Length; i++)
                {
                    if (char.ToLowerInvariant(s[i]) == char.ToLowerInvariant(s[i - 1]))
                    {
                        count++;
                        if (count > 3) return true;
                    }
                    else count = 1;
                }
                return false;
            }

            if (HasExcessiveRepeats(product.Name)) return BadRequest(new { error = "Name contains excessive repeated characters" });

            // Normalize name: Title case given names, UPPERCASE surname (last word)
            string NormalizeName(string name)
            {
                var parts = name.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return name;
                if (parts.Length == 1) return ToTitleCase(parts[0]);
                for (int i = 0; i < parts.Length - 1; i++) parts[i] = ToTitleCase(parts[i]);
                parts[parts.Length - 1] = parts[parts.Length - 1].ToUpperInvariant();
                return string.Join(' ', parts);
            }

            string ToTitleCase(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;
                if (s.Length == 1) return char.ToUpperInvariant(s[0]).ToString();
                return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
            }

            var normalized = NormalizeName(product.Name);

            // Check duplicates case-insensitive
            var all = await _repo.GetAllAsync();
            var exists = all.Any(p => p.Name.ToLower() == normalized.ToLower());
            if (exists) { _logger.LogWarning("Duplicate name attempted: {Name}", normalized); return BadRequest(new { error = "Duplicate name" }); }

            product.Name = normalized;
            product.CreatedDate = System.DateTime.UtcNow;
            var created = await _repo.CreateAsync(product);
            return Ok(created);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Product product)
        {
            var existing = await _repo.GetByIdAsync(product.Id);
            if (existing == null) { _logger.LogWarning("Update not found for id {Id}", product.Id); return NotFound(); }
            if (!ModelState.IsValid) { _logger.LogWarning("Invalid model state on Update for id {Id}", product.Id); return BadRequest(ModelState); }
            if (product.Stock < 0 || product.Stock > 100) return BadRequest(new { error = "Stock must be between 0 and 100" });

            if (string.IsNullOrWhiteSpace(product.Name)) return BadRequest(new { error = "Name is required" });
            if (product.Name.Any(char.IsDigit)) return BadRequest(new { error = "Name must not contain digits" });

            bool HasExcessiveRepeatsLocal(string s)
            {
                int count = 1;
                for (int i = 1; i < s.Length; i++)
                {
                    if (char.ToLowerInvariant(s[i]) == char.ToLowerInvariant(s[i - 1]))
                    {
                        count++;
                        if (count > 3) return true;
                    }
                    else count = 1;
                }
                return false;
            }

            if (HasExcessiveRepeatsLocal(product.Name)) return BadRequest(new { error = "Name contains excessive repeated characters" });

            string ToTitleCaseLocal(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;
                if (s.Length == 1) return char.ToUpperInvariant(s[0]).ToString();
                return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
            }

            string NormalizeNameLocal(string name)
            {
                var parts = name.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return name;
                if (parts.Length == 1) return ToTitleCaseLocal(parts[0]);
                for (int i = 0; i < parts.Length - 1; i++) parts[i] = ToTitleCaseLocal(parts[i]);
                parts[parts.Length - 1] = parts[parts.Length - 1].ToUpperInvariant();
                return string.Join(' ', parts);
            }

            var normalized = NormalizeNameLocal(product.Name);
            // Check duplicate names for other records (excluding current)
            var all = await _repo.GetAllAsync();
            var exists = all.Any(p => p.Id != product.Id && p.Name.ToLower() == normalized.ToLower());
            if (exists) { _logger.LogWarning("Duplicate name attempted on update: {Name}", normalized); return BadRequest(new { error = "Duplicate name" }); }

            existing.Name = normalized;
            existing.Stock = product.Stock;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(updated);
        }

        // PATCH api/Product/{id}/stock
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> SetStock(int id, [FromBody] StockDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) { _logger.LogWarning("SetStock not found for id {Id}", id); return NotFound(); }
            if (dto == null) { _logger.LogWarning("SetStock invalid payload for id {Id}", id); return BadRequest(new { error = "Invalid payload" }); }
            if (dto.Amount < 0 || dto.Amount > 100) return BadRequest(new { error = "Stock must be between 0 and 100" });
            existing.Stock = dto.Amount;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(updated);
        }

        // PATCH api/Product/{id}/stock/increment
        [HttpPatch("{id}/stock/increment")]
        public async Task<IActionResult> IncrementStock(int id, [FromBody] StockDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) { _logger.LogWarning("IncrementStock not found for id {Id}", id); return NotFound(); }
            if (dto == null) { _logger.LogWarning("IncrementStock invalid payload for id {Id}", id); return BadRequest(new { error = "Invalid payload" }); }
            var newStock = existing.Stock + dto.Amount;
            if (newStock < 0 || newStock > 100) return BadRequest(new { error = "Stock must be between 0 and 100" });
            existing.Stock = newStock;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(updated);
        }

        // PATCH api/Product/{id}/stock/decrement
        [HttpPatch("{id}/stock/decrement")]
        public async Task<IActionResult> DecrementStock(int id, [FromBody] StockDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) { _logger.LogWarning("DecrementStock not found for id {Id}", id); return NotFound(); }
            if (dto == null) { _logger.LogWarning("DecrementStock invalid payload for id {Id}", id); return BadRequest(new { error = "Invalid payload" }); }
            var newStock = existing.Stock - dto.Amount;
            if (newStock < 0 || newStock > 100) return BadRequest(new { error = "Stock must be between 0 and 100" });
            existing.Stock = newStock;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(updated);
        }

        public class StockDto { public int Amount { get; set; } }

        // DELETE api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) { _logger.LogWarning("Delete not found for id {Id}", id); return NotFound(); }
            var ok = await _repo.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
