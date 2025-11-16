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
    public class PuritiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PuritiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/purities
        [HttpGet]
        public async Task<ActionResult<object>> GetPurities(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? itemGroupId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Purities
                    .Include(p => p.ItemGroup)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.PurityName.Contains(searchTerm) ||
                        p.PurityCode.Contains(searchTerm) ||
                        (p.Description != null && p.Description.Contains(searchTerm)));
                }

                if (itemGroupId.HasValue)
                {
                    query = query.Where(p => p.ItemGroupId == itemGroupId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();

                var purities = await query
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.PurityCode,
                        p.PurityName,
                        p.PurityPercentage,
                        p.Karat,
                        p.ItemGroupId,
                        ItemGroupName = p.ItemGroup != null ? p.ItemGroup.GroupName : null,
                        p.Description,
                        p.IsActive,
                        p.CreatedDate,
                        p.UpdatedDate,
                        p.CreatedBy,
                        p.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Purities retrieved successfully",
                    data = new
                    {
                        purities,
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
                    message = "An error occurred while retrieving purities",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPurity(Guid id)
        {
            try
            {
                var purity = await _context.Purities
                    .Include(p => p.ItemGroup)
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.PurityCode,
                        p.PurityName,
                        p.PurityPercentage,
                        p.Karat,
                        p.ItemGroupId,
                        ItemGroupName = p.ItemGroup != null ? p.ItemGroup.GroupName : null,
                        p.Description,
                        p.IsActive,
                        p.CreatedDate,
                        p.UpdatedDate,
                        p.CreatedBy,
                        p.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (purity == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Purity not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Purity retrieved successfully",
                    data = purity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the purity",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<object>> GetPurityByCode(string code)
        {
            try
            {
                var purity = await _context.Purities
                    .Include(p => p.ItemGroup)
                    .Where(p => p.PurityCode == code)
                    .Select(p => new
                    {
                        p.Id,
                        p.PurityCode,
                        p.PurityName,
                        p.PurityPercentage,
                        p.Karat,
                        p.ItemGroupId,
                        ItemGroupName = p.ItemGroup != null ? p.ItemGroup.GroupName : null,
                        p.Description,
                        p.IsActive,
                        p.CreatedDate,
                        p.UpdatedDate,
                        p.CreatedBy,
                        p.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (purity == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Purity not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Purity retrieved successfully",
                    data = purity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the purity",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<object>> GetPuritiesByGroup(Guid groupId)
        {
            try
            {
                var purities = await _context.Purities
                    .Include(p => p.ItemGroup)
                    .Where(p => p.ItemGroupId == groupId)
                    .OrderBy(p => p.PurityPercentage)
                    .Select(p => new
                    {
                        p.Id,
                        p.PurityCode,
                        p.PurityName,
                        p.PurityPercentage,
                        p.Karat,
                        p.ItemGroupId,
                        ItemGroupName = p.ItemGroup != null ? p.ItemGroup.GroupName : null,
                        p.Description,
                        p.IsActive,
                        p.CreatedDate,
                        p.UpdatedDate,
                        p.CreatedBy,
                        p.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Purities retrieved successfully",
                    data = purities
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving purities",
                    error = ex.Message
                });
            }
        }

        // POST: api/purities
        [HttpPost]
        public async Task<ActionResult<object>> CreatePurity(Purity purity)
        {
            try
            {
                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == purity.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if purity code already exists
                var codeExists = await _context.Purities.AnyAsync(p => p.PurityCode == purity.PurityCode);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Purity code already exists"
                    });
                }

                // Validate purity percentage
                if (purity.PurityPercentage < 0 || purity.PurityPercentage > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Purity percentage must be between 0 and 100"
                    });
                }

                // Validate karat if provided
                if (purity.Karat.HasValue && (purity.Karat < 1 || purity.Karat > 24))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Karat must be between 1 and 24"
                    });
                }

                purity.Id = Guid.NewGuid();
                purity.CreatedDate = DateTime.UtcNow;
                purity.UpdatedDate = DateTime.UtcNow;
                purity.PurityCode = purity.PurityCode.ToUpper();

                _context.Purities.Add(purity);
                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(purity).Reference(p => p.ItemGroup).LoadAsync();

                return CreatedAtAction(nameof(GetPurity), new { id = purity.Id }, new
                {
                    success = true,
                    message = "Purity created successfully",
                    data = new
                    {
                        purity.Id,
                        purity.PurityCode,
                        purity.PurityName,
                        purity.PurityPercentage,
                        purity.Karat,
                        purity.ItemGroupId,
                        ItemGroupName = purity.ItemGroup?.GroupName,
                        purity.Description,
                        purity.IsActive,
                        purity.CreatedDate,
                        purity.UpdatedDate,
                        purity.CreatedBy,
                        purity.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the purity",
                    error = ex.Message
                });
            }
        }

        // PUT: api/purities/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdatePurity(Guid id, Purity purity)
        {
            if (id != purity.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID mismatch"
                });
            }

            try
            {
                var existingPurity = await _context.Purities.FindAsync(id);
                if (existingPurity == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Purity not found"
                    });
                }

                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == purity.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if purity code already exists for another purity
                var codeExists = await _context.Purities
                    .AnyAsync(p => p.PurityCode == purity.PurityCode && p.Id != id);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Purity code already exists"
                    });
                }

                // Validate purity percentage
                if (purity.PurityPercentage < 0 || purity.PurityPercentage > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Purity percentage must be between 0 and 100"
                    });
                }

                // Validate karat if provided
                if (purity.Karat.HasValue && (purity.Karat < 1 || purity.Karat > 24))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Karat must be between 1 and 24"
                    });
                }

                existingPurity.PurityCode = purity.PurityCode.ToUpper();
                existingPurity.PurityName = purity.PurityName;
                existingPurity.PurityPercentage = purity.PurityPercentage;
                existingPurity.Karat = purity.Karat;
                existingPurity.ItemGroupId = purity.ItemGroupId;
                existingPurity.Description = purity.Description;
                existingPurity.IsActive = purity.IsActive;
                existingPurity.UpdatedDate = DateTime.UtcNow;
                existingPurity.UpdatedBy = purity.UpdatedBy;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(existingPurity).Reference(p => p.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = "Purity updated successfully",
                    data = new
                    {
                        existingPurity.Id,
                        existingPurity.PurityCode,
                        existingPurity.PurityName,
                        existingPurity.PurityPercentage,
                        existingPurity.Karat,
                        existingPurity.ItemGroupId,
                        ItemGroupName = existingPurity.ItemGroup?.GroupName,
                        existingPurity.Description,
                        existingPurity.IsActive,
                        existingPurity.CreatedDate,
                        existingPurity.UpdatedDate,
                        existingPurity.CreatedBy,
                        existingPurity.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the purity",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/purities/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeletePurity(Guid id)
        {
            try
            {
                var purity = await _context.Purities.FindAsync(id);
                if (purity == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Purity not found"
                    });
                }

                _context.Purities.Remove(purity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Purity deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the purity",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/purities/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<object>> TogglePurityStatus(Guid id)
        {
            try
            {
                var purity = await _context.Purities.FindAsync(id);
                if (purity == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Purity not found"
                    });
                }

                purity.IsActive = !purity.IsActive;
                purity.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(purity).Reference(p => p.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Purity {(purity.IsActive ? "activated" : "deactivated")} successfully",
                    data = new
                    {
                        purity.Id,
                        purity.PurityCode,
                        purity.PurityName,
                        purity.PurityPercentage,
                        purity.Karat,
                        purity.ItemGroupId,
                        ItemGroupName = purity.ItemGroup?.GroupName,
                        purity.Description,
                        purity.IsActive,
                        purity.CreatedDate,
                        purity.UpdatedDate,
                        purity.CreatedBy,
                        purity.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while toggling purity status",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/generate-code
        [HttpGet("generate-code")]
        public async Task<ActionResult<object>> GeneratePurityCode()
        {
            try
            {
                // Get the last purity code
                var lastPurity = await _context.Purities
                    .OrderByDescending(p => p.PurityCode)
                    .FirstOrDefaultAsync();

                string newCode;
                if (lastPurity != null && lastPurity.PurityCode.StartsWith("PU"))
                {
                    // Extract number and increment
                    var numberPart = lastPurity.PurityCode.Substring(2);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        newCode = $"PU{(lastNumber + 1):D4}";
                    }
                    else
                    {
                        newCode = "PU0001";
                    }
                }
                else
                {
                    newCode = "PU0001";
                }

                return Ok(new
                {
                    success = true,
                    message = "Purity code generated successfully",
                    data = newCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating purity code",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/check-code/{code}
        [HttpGet("check-code/{code}")]
        public async Task<ActionResult<object>> CheckPurityCodeExists(string code)
        {
            try
            {
                var exists = await _context.Purities.AnyAsync(p => p.PurityCode == code.ToUpper());

                return Ok(new
                {
                    success = true,
                    message = "Code checked successfully",
                    data = exists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while checking purity code",
                    error = ex.Message
                });
            }
        }

        // GET: api/purities/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetPurityStatistics()
        {
            try
            {
                var totalPurities = await _context.Purities.CountAsync();
                var activePurities = await _context.Purities.CountAsync(p => p.IsActive);
                var inactivePurities = totalPurities - activePurities;

                var puritiesByGroup = await _context.Purities
                    .Include(p => p.ItemGroup)
                    .GroupBy(p => p.ItemGroup!.GroupName)
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
                        totalPurities,
                        activePurities,
                        inactivePurities,
                        puritiesByGroup
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