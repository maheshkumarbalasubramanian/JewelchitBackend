using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/itemtypes
        [HttpGet]
        public async Task<ActionResult<object>> GetItemTypes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? itemGroupId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.ItemTypes
                    .Include(i => i.ItemGroup)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(i =>
                        i.ItemName.Contains(searchTerm) ||
                        i.ItemCode.Contains(searchTerm) ||
                        (i.ItemNameTamil != null && i.ItemNameTamil.Contains(searchTerm)));
                }

                if (itemGroupId.HasValue)
                {
                    query = query.Where(i => i.ItemGroupId == itemGroupId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(i => i.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(i => i.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemCode,
                        i.ItemName,
                        i.ItemNameTamil,
                        i.ItemGroupId,
                        ItemGroupName = i.ItemGroup != null ? i.ItemGroup.GroupName : null,
                        i.Description,
                        i.IsActive,
                        i.ItemCount,
                        i.CreatedDate,
                        i.UpdatedDate,
                        i.CreatedBy,
                        i.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Item types retrieved successfully",
                    data = new
                    {
                        items,
                        total,
                        page,
                        pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving item types",
                    error = ex.Message
                });
            }
        }

        // GET: api/itemtypes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetItemType(Guid id)
        {
            try
            {
                var itemType = await _context.ItemTypes
                    .Include(i => i.ItemGroup)
                    .Where(i => i.Id == id)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemCode,
                        i.ItemName,
                        i.ItemNameTamil,
                        i.ItemGroupId,
                        ItemGroupName = i.ItemGroup != null ? i.ItemGroup.GroupName : null,
                        i.Description,
                        i.IsActive,
                        i.ItemCount,
                        i.CreatedDate,
                        i.UpdatedDate,
                        i.CreatedBy,
                        i.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (itemType == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item type not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Item type retrieved successfully",
                    data = itemType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the item type",
                    error = ex.Message
                });
            }
        }

        // GET: api/itemtypes/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<object>> GetItemTypeByCode(string code)
        {
            try
            {
                var itemType = await _context.ItemTypes
                    .Include(i => i.ItemGroup)
                    .Where(i => i.ItemCode == code)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemCode,
                        i.ItemName,
                        i.ItemNameTamil,
                        i.ItemGroupId,
                        ItemGroupName = i.ItemGroup != null ? i.ItemGroup.GroupName : null,
                        i.Description,
                        i.IsActive,
                        i.ItemCount,
                        i.CreatedDate,
                        i.UpdatedDate,
                        i.CreatedBy,
                        i.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (itemType == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item type not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Item type retrieved successfully",
                    data = itemType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the item type",
                    error = ex.Message
                });
            }
        }

        // GET: api/itemtypes/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<object>> GetItemTypesByGroup(Guid groupId)
        {
            try
            {
                var itemTypes = await _context.ItemTypes
                    .Include(i => i.ItemGroup)
                    .Where(i => i.ItemGroupId == groupId)
                    .OrderBy(i => i.ItemName)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemCode,
                        i.ItemName,
                        i.ItemNameTamil,
                        i.ItemGroupId,
                        ItemGroupName = i.ItemGroup != null ? i.ItemGroup.GroupName : null,
                        i.Description,
                        i.IsActive,
                        i.ItemCount,
                        i.CreatedDate,
                        i.UpdatedDate,
                        i.CreatedBy,
                        i.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Item types retrieved successfully",
                    data = itemTypes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving item types",
                    error = ex.Message
                });
            }
        }

        // POST: api/itemtypes
        [HttpPost]
        public async Task<ActionResult<object>> CreateItemType(ItemType itemType)
        {
            try
            {
                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == itemType.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if item code already exists
                var codeExists = await _context.ItemTypes.AnyAsync(i => i.ItemCode == itemType.ItemCode);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Item code already exists"
                    });
                }

                itemType.Id = Guid.NewGuid();
                itemType.CreatedDate = DateTime.UtcNow;
                itemType.UpdatedDate = DateTime.UtcNow;

                _context.ItemTypes.Add(itemType);
                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(itemType).Reference(i => i.ItemGroup).LoadAsync();

                return CreatedAtAction(nameof(GetItemType), new { id = itemType.Id }, new
                {
                    success = true,
                    message = "Item type created successfully",
                    data = new
                    {
                        itemType.Id,
                        itemType.ItemCode,
                        itemType.ItemName,
                        itemType.ItemNameTamil,
                        itemType.ItemGroupId,
                        ItemGroupName = itemType.ItemGroup?.GroupName,
                        itemType.Description,
                        itemType.IsActive,
                        itemType.ItemCount,
                        itemType.CreatedDate,
                        itemType.UpdatedDate,
                        itemType.CreatedBy,
                        itemType.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the item type",
                    error = ex.Message
                });
            }
        }

        // PUT: api/itemtypes/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateItemType(Guid id, ItemType itemType)
        {
            if (id != itemType.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID mismatch"
                });
            }

            try
            {
                var existingItemType = await _context.ItemTypes.FindAsync(id);
                if (existingItemType == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item type not found"
                    });
                }

                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == itemType.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if item code already exists for another item type
                var codeExists = await _context.ItemTypes
                    .AnyAsync(i => i.ItemCode == itemType.ItemCode && i.Id != id);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Item code already exists"
                    });
                }

                existingItemType.ItemCode = itemType.ItemCode;
                existingItemType.ItemName = itemType.ItemName;
                existingItemType.ItemNameTamil = itemType.ItemNameTamil;
                existingItemType.ItemGroupId = itemType.ItemGroupId;
                existingItemType.Description = itemType.Description;
                existingItemType.IsActive = itemType.IsActive;
                existingItemType.UpdatedDate = DateTime.UtcNow;
                existingItemType.UpdatedBy = itemType.UpdatedBy;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(existingItemType).Reference(i => i.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = "Item type updated successfully",
                    data = new
                    {
                        existingItemType.Id,
                        existingItemType.ItemCode,
                        existingItemType.ItemName,
                        existingItemType.ItemNameTamil,
                        existingItemType.ItemGroupId,
                        ItemGroupName = existingItemType.ItemGroup?.GroupName,
                        existingItemType.Description,
                        existingItemType.IsActive,
                        existingItemType.ItemCount,
                        existingItemType.CreatedDate,
                        existingItemType.UpdatedDate,
                        existingItemType.CreatedBy,
                        existingItemType.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the item type",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/itemtypes/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteItemType(Guid id)
        {
            try
            {
                var itemType = await _context.ItemTypes.FindAsync(id);
                if (itemType == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item type not found"
                    });
                }

                // Check if item type has any items (when you add items table later)
                // if (itemType.ItemCount > 0)
                // {
                //     return BadRequest(new
                //     {
                //         success = false,
                //         message = "Cannot delete item type with existing items"
                //     });
                // }

                _context.ItemTypes.Remove(itemType);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Item type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the item type",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/itemtypes/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<object>> ToggleItemTypeStatus(Guid id)
        {
            try
            {
                var itemType = await _context.ItemTypes.FindAsync(id);
                if (itemType == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item type not found"
                    });
                }

                itemType.IsActive = !itemType.IsActive;
                itemType.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(itemType).Reference(i => i.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Item type {(itemType.IsActive ? "activated" : "deactivated")} successfully",
                    data = new
                    {
                        itemType.Id,
                        itemType.ItemCode,
                        itemType.ItemName,
                        itemType.ItemNameTamil,
                        itemType.ItemGroupId,
                        ItemGroupName = itemType.ItemGroup?.GroupName,
                        itemType.Description,
                        itemType.IsActive,
                        itemType.ItemCount,
                        itemType.CreatedDate,
                        itemType.UpdatedDate,
                        itemType.CreatedBy,
                        itemType.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while toggling item type status",
                    error = ex.Message
                });
            }
        }

        // GET: api/itemtypes/generate-code
        [HttpGet("generate-code")]
        public ActionResult<object> GenerateItemCode()
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                var code = $"ITM{timestamp.Substring(timestamp.Length - 6)}";

                return Ok(new
                {
                    success = true,
                    message = "Item code generated successfully",
                    data = code
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating item code",
                    error = ex.Message
                });
            }
        }

        // GET: api/itemtypes/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetItemTypeStatistics()
        {
            try
            {
                var totalItemTypes = await _context.ItemTypes.CountAsync();
                var activeItemTypes = await _context.ItemTypes.CountAsync(i => i.IsActive);
                var inactiveItemTypes = totalItemTypes - activeItemTypes;

                var itemTypesByGroup = await _context.ItemTypes
                    .Include(i => i.ItemGroup)
                    .GroupBy(i => i.ItemGroup!.GroupName)
                    .Select(g => new
                    {
                        GroupName = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Statistics retrieved successfully",
                    data = new
                    {
                        totalItemTypes,
                        activeItemTypes,
                        inactiveItemTypes,
                        itemTypesByGroup
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving statistics",
                    error = ex.Message
                });
            }
        }
    }
}