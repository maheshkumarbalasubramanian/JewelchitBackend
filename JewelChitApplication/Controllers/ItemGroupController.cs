using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemGroupsController> _logger;

        public ItemGroupsController(
            ApplicationDbContext context,
            ILogger<ItemGroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/itemgroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemGroupResponse>>> GetItemGroups(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] Guid? areaId)
        {
            var query = _context.ItemGroups
                .Include(ig => ig.Area)
                    .ThenInclude(a => a!.Company)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(ig =>
                    ig.GroupName.ToLower().Contains(search.ToLower()) ||
                    ig.GroupCode.ToLower().Contains(search.ToLower()) ||
                    (ig.GroupNameTamil != null && ig.GroupNameTamil.Contains(search)) ||
                    (ig.Description != null && ig.Description.ToLower().Contains(search.ToLower())));
            }

            if (status == "active")
                query = query.Where(ig => ig.IsActive);
            else if (status == "inactive")
                query = query.Where(ig => !ig.IsActive);

            if (areaId.HasValue)
                query = query.Where(ig => ig.AreaId == areaId.Value);

            var itemGroups = await query
                .OrderBy(ig => ig.GroupName)
                .Select(ig => new ItemGroupResponse
                {
                    Id = ig.Id,
                    GroupCode = ig.GroupCode,
                    GroupName = ig.GroupName,
                    GroupNameTamil = ig.GroupNameTamil,
                    AreaId = ig.AreaId,
                    AreaName = ig.Area != null ? ig.Area.AreaName : null,
                    AreaCode = ig.Area != null ? ig.Area.AreaCode : null,
                    CompanyName = ig.Area != null && ig.Area.Company != null ? ig.Area.Company.CompanyName : null,
                    Description = ig.Description,
                    IsActive = ig.IsActive,
                    ItemCount = ig.ItemCount,
                    CreatedDate = ig.CreatedDate,
                    UpdatedDate = ig.UpdatedDate
                })
                .ToListAsync();

            return Ok(itemGroups);
        }

        // GET: api/itemgroups/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemGroupResponse>> GetItemGroup(Guid id)
        {
            var itemGroup = await _context.ItemGroups
                .Include(ig => ig.Area)
                    .ThenInclude(a => a!.Company)
                .FirstOrDefaultAsync(ig => ig.Id == id);

            if (itemGroup == null)
                return NotFound(new { message = "Item group not found" });

            var response = new ItemGroupResponse
            {
                Id = itemGroup.Id,
                GroupCode = itemGroup.GroupCode,
                GroupName = itemGroup.GroupName,
                GroupNameTamil = itemGroup.GroupNameTamil,
                AreaId = itemGroup.AreaId,
                AreaName = itemGroup.Area?.AreaName,
                AreaCode = itemGroup.Area?.AreaCode,
                CompanyName = itemGroup.Area?.Company?.CompanyName,
                Description = itemGroup.Description,
                IsActive = itemGroup.IsActive,
                ItemCount = itemGroup.ItemCount,
                CreatedDate = itemGroup.CreatedDate,
                UpdatedDate = itemGroup.UpdatedDate
            };

            return Ok(response);
        }

        // GET: api/itemgroups/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ItemGroupStatsResponse>> GetItemGroupStats()
        {
            var total = await _context.ItemGroups.CountAsync();
            var active = await _context.ItemGroups.CountAsync(ig => ig.IsActive);
            var inactive = total - active;
            var recent = await _context.ItemGroups
                .CountAsync(ig => ig.CreatedDate >= DateTime.UtcNow.AddDays(-7));

            return Ok(new ItemGroupStatsResponse
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Recent = recent
            });
        }

        // GET: api/itemgroups/by-area/{areaId}
        [HttpGet("by-area/{areaId}")]
        public async Task<ActionResult<IEnumerable<ItemGroupResponse>>> GetItemGroupsByArea(Guid areaId)
        {
            var itemGroups = await _context.ItemGroups
                .Include(ig => ig.Area)
                    .ThenInclude(a => a!.Company)
                .Where(ig => ig.AreaId == areaId)
                .Select(ig => new ItemGroupResponse
                {
                    Id = ig.Id,
                    GroupCode = ig.GroupCode,
                    GroupName = ig.GroupName,
                    GroupNameTamil = ig.GroupNameTamil,
                    AreaId = ig.AreaId,
                    AreaName = ig.Area != null ? ig.Area.AreaName : null,
                    AreaCode = ig.Area != null ? ig.Area.AreaCode : null,
                    CompanyName = ig.Area != null && ig.Area.Company != null ? ig.Area.Company.CompanyName : null,
                    Description = ig.Description,
                    IsActive = ig.IsActive,
                    ItemCount = ig.ItemCount,
                    CreatedDate = ig.CreatedDate,
                    UpdatedDate = ig.UpdatedDate
                })
                .ToListAsync();

            return Ok(itemGroups);
        }

        // GET: api/itemgroups/next-code
        [HttpGet("next-code")]
        public async Task<ActionResult<object>> GetNextGroupCode()
        {
            var lastGroup = await _context.ItemGroups
                .OrderByDescending(ig => ig.GroupCode)
                .FirstOrDefaultAsync();

            string nextCode;
            if (lastGroup == null)
            {
                nextCode = "IG0001";
            }
            else
            {
                // Extract numeric part and increment
                var numericPart = lastGroup.GroupCode.Substring(2);
                if (int.TryParse(numericPart, out int number))
                {
                    nextCode = $"IG{(number + 1):D4}";
                }
                else
                {
                    nextCode = "IG0001";
                }
            }

            return Ok(new { code = nextCode });
        }

        // GET: api/itemgroups/dropdown
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<ItemGroupDropdownResponse>>> GetItemGroupsDropdown()
        {
            var itemGroups = await _context.ItemGroups
                .Where(ig => ig.IsActive)
                .OrderBy(ig => ig.GroupName)
                .Select(ig => new ItemGroupDropdownResponse
                {
                    Id = ig.Id,
                    Code = ig.GroupCode,
                    Name = ig.GroupName
                })
                .ToListAsync();

            return Ok(itemGroups);
        }

        // POST: api/itemgroups
        [HttpPost]
        public async Task<ActionResult<ItemGroupResponse>> CreateItemGroup([FromBody] AddItemGroupRequest request)
        {
            // Check if area exists
            var areaExists = await _context.Areas.AnyAsync(a => a.Id == request.AreaId);
            if (!areaExists)
            {
                return BadRequest(new { message = "Area not found" });
            }

            // Check for duplicate group code
            if (await _context.ItemGroups.AnyAsync(ig => ig.GroupCode == request.GroupCode))
            {
                return Conflict(new { message = "Group code already exists" });
            }

            var itemGroup = new ItemGroup
            {
                Id = Guid.NewGuid(),
                GroupCode = request.GroupCode.ToUpper(),
                GroupName = request.GroupName,
                GroupNameTamil = request.GroupNameTamil,
                AreaId = request.AreaId,
                Description = request.Description,
                IsActive = true,
                ItemCount = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name
            };

            _context.ItemGroups.Add(itemGroup);
            await _context.SaveChangesAsync();

            var response = await GetItemGroup(itemGroup.Id);
            return CreatedAtAction(nameof(GetItemGroup), new { id = itemGroup.Id }, response.Value);
        }

        // PUT: api/itemgroups/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItemGroup(Guid id, [FromBody] UpdateItemGroupRequest request)
        {
            var itemGroup = await _context.ItemGroups.FindAsync(id);

            if (itemGroup == null)
                return NotFound(new { message = "Item group not found" });

            itemGroup.GroupName = request.GroupName;
            itemGroup.GroupNameTamil = request.GroupNameTamil;
            itemGroup.Description = request.Description;
            itemGroup.IsActive = request.IsActive;
            itemGroup.UpdatedDate = DateTime.UtcNow;
            itemGroup.UpdatedBy = User?.Identity?.Name;

            _context.Entry(itemGroup).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/itemgroups/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemGroup(Guid id)
        {
            var itemGroup = await _context.ItemGroups.FindAsync(id);

            if (itemGroup == null)
                return NotFound(new { message = "Item group not found" });

            // Check if item group has items
            if (itemGroup.ItemCount > 0)
            {
                return BadRequest(new { message = "Cannot delete item group with existing items" });
            }

            _context.ItemGroups.Remove(itemGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/itemgroups/bulk-activate
        [HttpPost("bulk-activate")]
        public async Task<IActionResult> BulkActivate([FromBody] List<Guid> ids)
        {
            var itemGroups = await _context.ItemGroups
                .Where(ig => ids.Contains(ig.Id))
                .ToListAsync();

            foreach (var itemGroup in itemGroups)
            {
                itemGroup.IsActive = true;
                itemGroup.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{itemGroups.Count} item groups activated" });
        }

        // POST: api/itemgroups/bulk-deactivate
        [HttpPost("bulk-deactivate")]
        public async Task<IActionResult> BulkDeactivate([FromBody] List<Guid> ids)
        {
            var itemGroups = await _context.ItemGroups
                .Where(ig => ids.Contains(ig.Id))
                .ToListAsync();

            foreach (var itemGroup in itemGroups)
            {
                itemGroup.IsActive = false;
                itemGroup.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{itemGroups.Count} item groups deactivated" });
        }
    }
}