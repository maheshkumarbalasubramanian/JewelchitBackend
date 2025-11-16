using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchemesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/schemes
        [HttpGet]
        public async Task<ActionResult<object>> GetSchemes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? areaId = null,
            [FromQuery] Guid? itemGroupId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(s =>
                        s.SchemeName.Contains(searchTerm) ||
                        s.SchemeCode.Contains(searchTerm) ||
                        s.Area!.AreaName.Contains(searchTerm) ||
                        s.ItemGroup!.GroupName.Contains(searchTerm));
                }

                if (areaId.HasValue)
                {
                    query = query.Where(s => s.AreaId == areaId.Value);
                }

                if (itemGroupId.HasValue)
                {
                    query = query.Where(s => s.ItemGroupId == itemGroupId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();

                var schemes = await query
                    .OrderByDescending(s => s.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.AreaId,
                        AreaCode = s.Area!.AreaCode,
                        AreaName = s.Area.AreaName,
                        s.ItemGroupId,
                        ItemGroupCode = s.ItemGroup!.GroupCode,
                        ItemGroupName = s.ItemGroup.GroupName,
                        s.Roi,
                        s.CalculationMethod,
                        s.IsStdRoi,
                        s.CalculationBased,
                        s.CustomizedStyle,
                        s.ProcessingFeeSlab,
                        s.MinCalcDays,
                        s.GraceDays,
                        s.AdvanceMonth,
                        s.ProcessingFeePercent,
                        s.MinMarketValue,
                        s.MaxMarketValue,
                        s.MinLoanValue,
                        s.MaxLoanValue,
                        s.PenaltyRate,
                        s.PenaltyGraceDays,
                        s.CompoundingFrequency,
                        s.EmiTenure,
                        s.ReductionPercent,
                        s.ValidityInMonths,
                        s.InterestPercentAfterValidity,
                        s.IsActive,
                        s.CreatedDate,
                        s.UpdatedDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Schemes retrieved successfully",
                    data = new
                    {
                        schemes,
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
                    message = "Error retrieving schemes",
                    error = ex.Message
                });
            }
        }

        // GET: api/schemes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetScheme(Guid id)
        {
            try
            {
                var scheme = await _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .Where(s => s.Id == id)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.AreaId,
                        AreaCode = s.Area!.AreaCode,
                        AreaName = s.Area.AreaName,
                        s.ItemGroupId,
                        ItemGroupCode = s.ItemGroup!.GroupCode,
                        ItemGroupName = s.ItemGroup.GroupName,
                        s.Roi,
                        s.CalculationMethod,
                        s.IsStdRoi,
                        s.CalculationBased,
                        s.CustomizedStyle,
                        s.ProcessingFeeSlab,
                        s.MinCalcDays,
                        s.GraceDays,
                        s.AdvanceMonth,
                        s.ProcessingFeePercent,
                        s.MinMarketValue,
                        s.MaxMarketValue,
                        s.MinLoanValue,
                        s.MaxLoanValue,
                        s.PenaltyRate,
                        s.PenaltyGraceDays,
                        s.CompoundingFrequency,
                        s.EmiTenure,
                        s.ReductionPercent,
                        s.ValidityInMonths,
                        s.InterestPercentAfterValidity,
                        s.IsActive,
                        s.CreatedDate,
                        s.UpdatedDate
                    })
                    .FirstOrDefaultAsync();

                if (scheme == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Scheme not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Scheme retrieved successfully",
                    data = scheme
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving scheme",
                    error = ex.Message
                });
            }
        }

        // GET: api/schemes/area/{areaId}
        [HttpGet("area/{areaId}")]
        public async Task<ActionResult<object>> GetSchemesByArea(Guid areaId)
        {
            try
            {
                var schemes = await _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .Where(s => s.AreaId == areaId && s.IsActive)
                    .OrderBy(s => s.SchemeName)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.AreaId,
                        AreaName = s.Area!.AreaName,
                        s.ItemGroupId,
                        ItemGroupName = s.ItemGroup!.GroupName,
                        s.Roi,
                        s.CalculationMethod,
                        s.MinLoanValue,
                        s.MaxLoanValue
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Schemes for area retrieved successfully",
                    data = schemes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving schemes by area",
                    error = ex.Message
                });
            }
        }

        // GET: api/schemes/itemgroup/{itemGroupId}
        [HttpGet("itemgroup/{itemGroupId}")]
        public async Task<ActionResult<object>> GetSchemesByItemGroup(Guid itemGroupId)
        {
            try
            {
                var schemes = await _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .Where(s => s.ItemGroupId == itemGroupId && s.IsActive)
                    .OrderBy(s => s.SchemeName)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.AreaId,
                        AreaName = s.Area!.AreaName,
                        s.ItemGroupId,
                        ItemGroupName = s.ItemGroup!.GroupName,
                        s.Roi,
                        s.CalculationMethod,
                        s.MinLoanValue,
                        s.MaxLoanValue
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Schemes for item group retrieved successfully",
                    data = schemes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving schemes by item group",
                    error = ex.Message
                });
            }
        }

        // GET: api/schemes/active
        [HttpGet("active")]
        public async Task<ActionResult<object>> GetActiveSchemes()
        {
            try
            {
                var schemes = await _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SchemeName)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        AreaName = s.Area!.AreaName,
                        ItemGroupName = s.ItemGroup!.GroupName,
                        s.Roi
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Active schemes retrieved successfully",
                    data = schemes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving active schemes",
                    error = ex.Message
                });
            }
        }

        // GET: api/schemes/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<object>> GetSchemeByCode(string code)
        {
            try
            {
                var scheme = await _context.Schemes
                    .Include(s => s.Area)
                    .Include(s => s.ItemGroup)
                    .Where(s => s.SchemeCode == code)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.AreaId,
                        AreaCode = s.Area!.AreaCode,
                        AreaName = s.Area.AreaName,
                        s.ItemGroupId,
                        ItemGroupCode = s.ItemGroup!.GroupCode,
                        ItemGroupName = s.ItemGroup.GroupName,
                        s.Roi,
                        s.CalculationMethod,
                        s.IsStdRoi,
                        s.CalculationBased,
                        s.CustomizedStyle,
                        s.ProcessingFeeSlab,
                        s.MinCalcDays,
                        s.GraceDays,
                        s.AdvanceMonth,
                        s.ProcessingFeePercent,
                        s.MinMarketValue,
                        s.MaxMarketValue,
                        s.MinLoanValue,
                        s.MaxLoanValue,
                        s.PenaltyRate,
                        s.PenaltyGraceDays,
                        s.CompoundingFrequency,
                        s.EmiTenure,
                        s.ReductionPercent,
                        s.ValidityInMonths,
                        s.InterestPercentAfterValidity,
                        s.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (scheme == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Scheme not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Scheme retrieved successfully",
                    data = scheme
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving scheme",
                    error = ex.Message
                });
            }
        }

        // POST: api/schemes
        [HttpPost]
        public async Task<ActionResult<object>> CreateScheme([FromBody] Scheme scheme)
        {
            try
            {
                // Validate area exists
                var areaExists = await _context.Areas.AnyAsync(a => a.Id == scheme.AreaId);
                if (!areaExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid area ID"
                    });
                }

                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(ig => ig.Id == scheme.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group ID"
                    });
                }

                // Check if scheme code already exists
                var codeExists = await _context.Schemes.AnyAsync(s => s.SchemeCode == scheme.SchemeCode);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Scheme code already exists"
                    });
                }

                scheme.Id = Guid.NewGuid();
                scheme.CreatedDate = DateTime.UtcNow;
                scheme.UpdatedDate = DateTime.UtcNow;

                _context.Schemes.Add(scheme);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetScheme), new { id = scheme.Id }, new
                {
                    success = true,
                    message = "Scheme created successfully",
                    data = new { scheme.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error creating scheme",
                    error = ex.Message
                });
            }
        }

        // PUT: api/schemes/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateScheme(Guid id, [FromBody] Scheme scheme)
        {
            try
            {
                if (id != scheme.Id)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID mismatch"
                    });
                }

                var existingScheme = await _context.Schemes.FindAsync(id);
                if (existingScheme == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Scheme not found"
                    });
                }

                // Validate area exists
                var areaExists = await _context.Areas.AnyAsync(a => a.Id == scheme.AreaId);
                if (!areaExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid area ID"
                    });
                }

                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(ig => ig.Id == scheme.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group ID"
                    });
                }

                // Check if scheme code already exists (excluding current scheme)
                var codeExists = await _context.Schemes.AnyAsync(s =>
                    s.SchemeCode == scheme.SchemeCode && s.Id != id);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Scheme code already exists"
                    });
                }

                // Update properties
                existingScheme.SchemeCode = scheme.SchemeCode;
                existingScheme.SchemeName = scheme.SchemeName;
                existingScheme.AreaId = scheme.AreaId;
                existingScheme.ItemGroupId = scheme.ItemGroupId;
                existingScheme.Roi = scheme.Roi;
                existingScheme.CalculationMethod = scheme.CalculationMethod;
                existingScheme.IsStdRoi = scheme.IsStdRoi;
                existingScheme.CalculationBased = scheme.CalculationBased;
                existingScheme.CustomizedStyle = scheme.CustomizedStyle;
                existingScheme.ProcessingFeeSlab = scheme.ProcessingFeeSlab;
                existingScheme.MinCalcDays = scheme.MinCalcDays;
                existingScheme.GraceDays = scheme.GraceDays;
                existingScheme.AdvanceMonth = scheme.AdvanceMonth;
                existingScheme.ProcessingFeePercent = scheme.ProcessingFeePercent;
                existingScheme.MinMarketValue = scheme.MinMarketValue;
                existingScheme.MaxMarketValue = scheme.MaxMarketValue;
                existingScheme.MinLoanValue = scheme.MinLoanValue;
                existingScheme.MaxLoanValue = scheme.MaxLoanValue;
                existingScheme.PenaltyRate = scheme.PenaltyRate;
                existingScheme.PenaltyGraceDays = scheme.PenaltyGraceDays;
                existingScheme.CompoundingFrequency = scheme.CompoundingFrequency;
                existingScheme.EmiTenure = scheme.EmiTenure;
                existingScheme.ReductionPercent = scheme.ReductionPercent;
                existingScheme.ValidityInMonths = scheme.ValidityInMonths;
                existingScheme.InterestPercentAfterValidity = scheme.InterestPercentAfterValidity;
                existingScheme.IsActive = scheme.IsActive;
                existingScheme.UpdatedDate = DateTime.UtcNow;
                existingScheme.UpdatedBy = scheme.UpdatedBy;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Scheme updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating scheme",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/schemes/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteScheme(Guid id)
        {
            try
            {
                var scheme = await _context.Schemes.FindAsync(id);
                if (scheme == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Scheme not found"
                    });
                }

                _context.Schemes.Remove(scheme);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Scheme deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting scheme",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/schemes/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<object>> ToggleSchemeStatus(Guid id)
        {
            try
            {
                var scheme = await _context.Schemes.FindAsync(id);
                if (scheme == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Scheme not found"
                    });
                }

                scheme.IsActive = !scheme.IsActive;
                scheme.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Scheme {(scheme.IsActive ? "activated" : "deactivated")} successfully",
                    data = new { scheme.IsActive }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error toggling scheme status",
                    error = ex.Message
                });
            }
        }
    }
}