using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateSetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RateSetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ratesets
        [HttpGet]
        public async Task<ActionResult<object>> GetRateSets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? areaId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.RateSets
                    .Include(r => r.Area)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(r =>
                        r.Area!.AreaName.Contains(searchTerm) ||
                        r.Area.AreaCode.Contains(searchTerm) ||
                        (r.Notes != null && r.Notes.Contains(searchTerm)));
                }

                if (areaId.HasValue)
                {
                    query = query.Where(r => r.AreaId == areaId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(r => r.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();

                var rateSets = await query
                    .OrderByDescending(r => r.EffectiveDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rate sets retrieved successfully",
                    data = new
                    {
                        rateSets,
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
                    message = "Error retrieving rate sets",
                    error = ex.Message
                });
            }
        }

        // GET: api/ratesets/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRateSet(Guid id)
        {
            try
            {
                var rateSet = await _context.RateSets
                    .Include(r => r.Area)
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .FirstOrDefaultAsync();

                if (rateSet == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rate set not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Rate set retrieved successfully",
                    data = rateSet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving rate set",
                    error = ex.Message
                });
            }
        }

        // GET: api/ratesets/area/{areaId}/current
        [HttpGet("area/{areaId}/current")]
        public async Task<ActionResult<object>> GetCurrentRateSetByArea(Guid areaId)
        {
            try
            {
                var rateSet = await _context.RateSets
                    .Include(r => r.Area)
                    .Where(r => r.AreaId == areaId && r.IsActive)
                    .OrderByDescending(r => r.EffectiveDate)
                    .ThenByDescending(r => r.CreatedDate) // Get latest if multiple on same date
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .FirstOrDefaultAsync();

                if (rateSet == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No active rate set found for this area"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Current rate set retrieved successfully",
                    data = rateSet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving current rate set",
                    error = ex.Message
                });
            }
        }

        // GET: api/ratesets/area/{areaId}
        [HttpGet("area/{areaId}")]
        public async Task<ActionResult<object>> GetRateSetsByArea(Guid areaId)
        {
            try
            {
                var rateSets = await _context.RateSets
                    .Include(r => r.Area)
                    .Where(r => r.AreaId == areaId)
                    .OrderByDescending(r => r.EffectiveDate)
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rate sets retrieved successfully",
                    data = rateSets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving rate sets",
                    error = ex.Message
                });
            }
        }

        // POST: api/ratesets
        [HttpPost]
        public async Task<ActionResult<object>> CreateRateSet([FromBody] RateSet rateSet)
        {
            try
            {
                // Validate that effective date is not in the past
                if (rateSet.EffectiveDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot create rate set for past dates. Please select today or a future date."
                    });
                }

                // Check if area exists
                var areaExists = await _context.Areas.AnyAsync(a => a.Id == rateSet.AreaId);
                if (!areaExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid area ID"
                    });
                }

                // Check for duplicate rate set (same area and date)
                //var duplicateExists = await _context.RateSets
                //    .AnyAsync(r => r.AreaId == rateSet.AreaId &&
                //                   r.EffectiveDate.Date == rateSet.EffectiveDate.Date);

                //if (duplicateExists)
                //{
                //    return Conflict(new
                //    {
                //        success = false,
                //        message = "A rate set for this area and date already exists. Please update the existing rate set or choose a different date."
                //    });
                //}

                rateSet.Id = Guid.NewGuid();
                rateSet.CreatedDate = DateTime.UtcNow;
                rateSet.UpdatedDate = DateTime.UtcNow;
                rateSet.IsActive = true;

                _context.RateSets.Add(rateSet);
                await _context.SaveChangesAsync();

                var createdRateSet = await _context.RateSets
                    .Include(r => r.Area)
                    .Where(r => r.Id == rateSet.Id)
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetRateSet), new { id = rateSet.Id }, new
                {
                    success = true,
                    message = "Rate set created successfully",
                    data = createdRateSet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error creating rate set",
                    error = ex.Message
                });
            }
        }

        // PUT: api/ratesets/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateRateSet(Guid id, [FromBody] RateSet rateSet)
        {
            if (id != rateSet.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID mismatch"
                });
            }

            try
            {
                var existingRateSet = await _context.RateSets.FindAsync(id);
                if (existingRateSet == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rate set not found"
                    });
                }

                // Check if area exists
                var areaExists = await _context.Areas.AnyAsync(a => a.Id == rateSet.AreaId);
                if (!areaExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid area ID"
                    });
                }

                // Check for duplicate only if area or date is being changed
                // Exclude the current record being updated
                //var duplicateExists = await _context.RateSets
                //    .AnyAsync(r => r.AreaId == rateSet.AreaId &&
                //                   r.EffectiveDate.Date == rateSet.EffectiveDate.Date &&
                //                   r.Id != id);

                //if (duplicateExists)
                //{
                //    return Conflict(new
                //    {
                //        success = false,
                //        message = "A different rate set for this area and date already exists. Please choose a different date."
                //    });
                //}

                // Update fields
                existingRateSet.AreaId = rateSet.AreaId;
                existingRateSet.EffectiveDate = rateSet.EffectiveDate;
                existingRateSet.GoldRatePerGram = rateSet.GoldRatePerGram;
                existingRateSet.GoldPurityRatePerGram = rateSet.GoldPurityRatePerGram;
                existingRateSet.SilverRatePerGram = rateSet.SilverRatePerGram;
                existingRateSet.SilverPurityRatePerGram = rateSet.SilverPurityRatePerGram;
                existingRateSet.Notes = rateSet.Notes;
                existingRateSet.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedRateSet = await _context.RateSets
                    .Include(r => r.Area)
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        r.Id,
                        r.AreaId,
                        AreaCode = r.Area!.AreaCode,
                        AreaName = r.Area.AreaName,
                        r.EffectiveDate,
                        r.GoldRatePerGram,
                        r.GoldPurityRatePerGram,
                        r.SilverRatePerGram,
                        r.SilverPurityRatePerGram,
                        r.Notes,
                        r.IsActive,
                        r.CreatedDate,
                        r.UpdatedDate
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rate set updated successfully",
                    data = updatedRateSet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating rate set",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/ratesets/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteRateSet(Guid id)
        {
            try
            {
                var rateSet = await _context.RateSets.FindAsync(id);
                if (rateSet == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rate set not found"
                    });
                }

                _context.RateSets.Remove(rateSet);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rate set deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting rate set",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/ratesets/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<object>> ToggleRateSetStatus(Guid id)
        {
            try
            {
                var rateSet = await _context.RateSets.FindAsync(id);
                if (rateSet == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rate set not found"
                    });
                }

                rateSet.IsActive = !rateSet.IsActive;
                rateSet.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Rate set {(rateSet.IsActive ? "activated" : "deactivated")} successfully",
                    data = rateSet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error toggling rate set status",
                    error = ex.Message
                });
            }
        }

        // GET: api/ratesets/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var total = await _context.RateSets.CountAsync();
                var active = await _context.RateSets.CountAsync(r => r.IsActive);
                var inactive = await _context.RateSets.CountAsync(r => !r.IsActive);
                var recent = await _context.RateSets
                    .CountAsync(r => r.CreatedDate >= DateTime.UtcNow.AddDays(-7));

                return Ok(new
                {
                    success = true,
                    message = "Statistics retrieved successfully",
                    data = new
                    {
                        total,
                        active,
                        inactive,
                        recent
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving statistics",
                    error = ex.Message
                });
            }
        }
    }
}