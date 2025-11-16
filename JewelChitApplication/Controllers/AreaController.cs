using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AreasController> _logger;

        public AreasController(
            ApplicationDbContext context,
            ILogger<AreasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/areas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AreaResponse>>> GetAreas(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? type,
            [FromQuery] Guid? companyId,
            [FromQuery] int? minCustomers,
            [FromQuery] int? maxCustomers)
        {
            var query = _context.Areas.Include(a => a.Company).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.AreaName.ToLower().Contains(search.ToLower()) ||
                    a.AreaCode.ToLower().Contains(search.ToLower()) ||
                    (a.Description != null && a.Description.ToLower().Contains(search.ToLower())));
            }

            if (status == "active")
                query = query.Where(a => a.IsActive);
            else if (status == "inactive")
                query = query.Where(a => !a.IsActive);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(a => a.AreaType == type);

            if (companyId.HasValue)
                query = query.Where(a => a.CompanyId == companyId.Value);

            if (minCustomers.HasValue)
                query = query.Where(a => a.CustomerCount >= minCustomers.Value);

            if (maxCustomers.HasValue)
                query = query.Where(a => a.CustomerCount <= maxCustomers.Value);

            var areas = await query
                .OrderBy(a => a.AreaName)
                .Select(a => new AreaResponse
                {
                    Id = a.Id,
                    AreaCode = a.AreaCode,
                    AreaName = a.AreaName,
                    AreaType = a.AreaType,
                    CompanyId = a.CompanyId,
                    CompanyName = a.Company != null ? a.Company.CompanyName : null,
                    CompanyCode = a.Company != null ? a.Company.CompanyCode : null,
                    Description = a.Description,
                    Pincode = a.Pincode,
                    IsActive = a.IsActive,
                    CustomerCount = a.CustomerCount,
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();

            return Ok(areas);
        }

        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<AreaDropdownResponse>>> GetAreasDropdown()
        {
            var areas = await _context.Areas
                .Include(a => a.Company)
                .Where(a => a.IsActive)
                .OrderBy(a => a.AreaName)
                .Select(a => new AreaDropdownResponse
                {
                    Id = a.Id,
                    Code = a.AreaCode,
                    Name = a.AreaName,
                    CompanyName = a.Company != null ? a.Company.CompanyName : ""
                })
                .ToListAsync();

            return Ok(areas);
        }

        // GET: api/areas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AreaResponse>> GetArea(Guid id)
        {
            var area = await _context.Areas
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (area == null)
                return NotFound(new { message = "Area not found" });

            var response = new AreaResponse
            {
                Id = area.Id,
                AreaCode = area.AreaCode,
                AreaName = area.AreaName,
                AreaType = area.AreaType,
                CompanyId = area.CompanyId,
                CompanyName = area.Company?.CompanyName,
                CompanyCode = area.Company?.CompanyCode,
                Description = area.Description,
                Pincode = area.Pincode,
                IsActive = area.IsActive,
                CustomerCount = area.CustomerCount,
                CreatedDate = area.CreatedDate,
                UpdatedDate = area.UpdatedDate
            };

            return Ok(response);
        }

        // GET: api/areas/stats
        [HttpGet("stats")]
        public async Task<ActionResult<AreaStatsResponse>> GetAreaStats()
        {
            var total = await _context.Areas.CountAsync();
            var active = await _context.Areas.CountAsync(a => a.IsActive);
            var inactive = total - active;
            var recent = await _context.Areas
                .CountAsync(a => a.CreatedDate >= DateTime.UtcNow.AddDays(-7));

            return Ok(new AreaStatsResponse
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Recent = recent
            });
        }

        // GET: api/areas/by-company/{companyId}
        [HttpGet("by-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<AreaResponse>>> GetAreasByCompany(Guid companyId)
        {
            var areas = await _context.Areas
                .Include(a => a.Company)
                .Where(a => a.CompanyId == companyId)
                .Select(a => new AreaResponse
                {
                    Id = a.Id,
                    AreaCode = a.AreaCode,
                    AreaName = a.AreaName,
                    AreaType = a.AreaType,
                    CompanyId = a.CompanyId,
                    CompanyName = a.Company != null ? a.Company.CompanyName : null,
                    CompanyCode = a.Company != null ? a.Company.CompanyCode : null,
                    Description = a.Description,
                    Pincode = a.Pincode,
                    IsActive = a.IsActive,
                    CustomerCount = a.CustomerCount,
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();

            return Ok(areas);
        }

        // POST: api/areas
        [HttpPost]
        public async Task<ActionResult<AreaResponse>> CreateArea([FromBody] AddAreaRequest request)
        {
            // Check if company exists
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId);
            if (!companyExists)
            {
                return BadRequest(new { message = "Company not found" });
            }

            // Check for duplicate area code
            if (await _context.Areas.AnyAsync(a => a.AreaCode == request.AreaCode))
            {
                return Conflict(new { message = "Area code already exists" });
            }

            var area = new Area
            {
                Id = Guid.NewGuid(),
                AreaCode = request.AreaCode.ToUpper(),
                AreaName = request.AreaName,
                AreaType = request.AreaType,
                CompanyId = request.CompanyId,
                Description = request.Description,
                Pincode = request.Pincode,
                IsActive = true,
                CustomerCount = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            var response = await GetArea(area.Id);
            return CreatedAtAction(nameof(GetArea), new { id = area.Id }, response.Value);
        }

        // PUT: api/areas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArea(Guid id, [FromBody] UpdateAreaRequest request)
        {
            var area = await _context.Areas.FindAsync(id);

            if (area == null)
                return NotFound(new { message = "Area not found" });

            area.AreaName = request.AreaName;
            area.AreaType = request.AreaType;
            area.Description = request.Description;
            area.Pincode = request.Pincode;
            area.IsActive = request.IsActive;
            area.UpdatedDate = DateTime.UtcNow;
            area.UpdatedBy = User?.Identity?.Name;

            _context.Entry(area).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/areas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArea(Guid id)
        {
            var area = await _context.Areas.FindAsync(id);

            if (area == null)
                return NotFound(new { message = "Area not found" });

            // Check if area has customers
            if (area.CustomerCount > 0)
            {
                return BadRequest(new { message = "Cannot delete area with existing customers" });
            }

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/areas/bulk-activate
        [HttpPost("bulk-activate")]
        public async Task<IActionResult> BulkActivate([FromBody] List<Guid> ids)
        {
            var areas = await _context.Areas
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            foreach (var area in areas)
            {
                area.IsActive = true;
                area.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{areas.Count} areas activated" });
        }

        // POST: api/areas/bulk-deactivate
        [HttpPost("bulk-deactivate")]
        public async Task<IActionResult> BulkDeactivate([FromBody] List<Guid> ids)
        {
            var areas = await _context.Areas
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            foreach (var area in areas)
            {
                area.IsActive = false;
                area.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{areas.Count} areas deactivated" });
        }
    }
}