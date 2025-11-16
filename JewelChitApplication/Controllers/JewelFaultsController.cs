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
    public class JewelFaultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JewelFaultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/jewelfaults
        [HttpGet]
        public async Task<ActionResult<object>> GetJewelFaults(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? itemGroupId = null,
            [FromQuery] string? faultType = null,
            [FromQuery] string? severity = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.JewelFaults
                    .Include(f => f.ItemGroup)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(f =>
                        f.FaultName.Contains(searchTerm) ||
                        f.FaultCode.Contains(searchTerm) ||
                        (f.Description != null && f.Description.Contains(searchTerm)));
                }

                if (itemGroupId.HasValue)
                {
                    query = query.Where(f => f.ItemGroupId == itemGroupId.Value);
                }

                if (!string.IsNullOrWhiteSpace(faultType))
                {
                    query = query.Where(f => f.FaultType == faultType.ToUpper());
                }

                if (!string.IsNullOrWhiteSpace(severity))
                {
                    query = query.Where(f => f.Severity == severity.ToUpper());
                }

                if (isActive.HasValue)
                {
                    query = query.Where(f => f.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();

                var faults = await query
                    .OrderByDescending(f => f.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.Id,
                        f.FaultCode,
                        f.FaultName,
                        f.FaultType,
                        f.Description,
                        f.Severity,
                        f.AffectsValuation,
                        f.ValuationImpactPercentage,
                        f.ItemGroupId,
                        ItemGroupName = f.ItemGroup != null ? f.ItemGroup.GroupName : null,
                        f.IsActive,
                        f.CreatedDate,
                        f.UpdatedDate,
                        f.CreatedBy,
                        f.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Jewel faults retrieved successfully",
                    data = new
                    {
                        faults,
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
                    message = "An error occurred while retrieving jewel faults",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetJewelFault(Guid id)
        {
            try
            {
                var fault = await _context.JewelFaults
                    .Include(f => f.ItemGroup)
                    .Where(f => f.Id == id)
                    .Select(f => new
                    {
                        f.Id,
                        f.FaultCode,
                        f.FaultName,
                        f.FaultType,
                        f.Description,
                        f.Severity,
                        f.AffectsValuation,
                        f.ValuationImpactPercentage,
                        f.ItemGroupId,
                        ItemGroupName = f.ItemGroup != null ? f.ItemGroup.GroupName : null,
                        f.IsActive,
                        f.CreatedDate,
                        f.UpdatedDate,
                        f.CreatedBy,
                        f.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (fault == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Jewel fault not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Jewel fault retrieved successfully",
                    data = fault
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the jewel fault",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<object>> GetJewelFaultByCode(string code)
        {
            try
            {
                var fault = await _context.JewelFaults
                    .Include(f => f.ItemGroup)
                    .Where(f => f.FaultCode == code)
                    .Select(f => new
                    {
                        f.Id,
                        f.FaultCode,
                        f.FaultName,
                        f.FaultType,
                        f.Description,
                        f.Severity,
                        f.AffectsValuation,
                        f.ValuationImpactPercentage,
                        f.ItemGroupId,
                        ItemGroupName = f.ItemGroup != null ? f.ItemGroup.GroupName : null,
                        f.IsActive,
                        f.CreatedDate,
                        f.UpdatedDate,
                        f.CreatedBy,
                        f.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (fault == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Jewel fault not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Jewel fault retrieved successfully",
                    data = fault
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the jewel fault",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<object>> GetJewelFaultsByGroup(Guid groupId)
        {
            try
            {
                var faults = await _context.JewelFaults
                    .Include(f => f.ItemGroup)
                    .Where(f => f.ItemGroupId == groupId)
                    .OrderBy(f => f.FaultName)
                    .Select(f => new
                    {
                        f.Id,
                        f.FaultCode,
                        f.FaultName,
                        f.FaultType,
                        f.Description,
                        f.Severity,
                        f.AffectsValuation,
                        f.ValuationImpactPercentage,
                        f.ItemGroupId,
                        ItemGroupName = f.ItemGroup != null ? f.ItemGroup.GroupName : null,
                        f.IsActive,
                        f.CreatedDate,
                        f.UpdatedDate,
                        f.CreatedBy,
                        f.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Jewel faults retrieved successfully",
                    data = faults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving jewel faults",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/severity/{severity}
        [HttpGet("severity/{severity}")]
        public async Task<ActionResult<object>> GetJewelFaultsBySeverity(string severity)
        {
            try
            {
                var faults = await _context.JewelFaults
                    .Include(f => f.ItemGroup)
                    .Where(f => f.Severity == severity.ToUpper())
                    .OrderBy(f => f.FaultName)
                    .Select(f => new
                    {
                        f.Id,
                        f.FaultCode,
                        f.FaultName,
                        f.FaultType,
                        f.Description,
                        f.Severity,
                        f.AffectsValuation,
                        f.ValuationImpactPercentage,
                        f.ItemGroupId,
                        ItemGroupName = f.ItemGroup != null ? f.ItemGroup.GroupName : null,
                        f.IsActive,
                        f.CreatedDate,
                        f.UpdatedDate,
                        f.CreatedBy,
                        f.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Jewel faults retrieved successfully",
                    data = faults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving jewel faults",
                    error = ex.Message
                });
            }
        }

        // POST: api/jewelfaults
        [HttpPost]
        public async Task<ActionResult<object>> CreateJewelFault(JewelFault fault)
        {
            try
            {
                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == fault.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if fault code already exists
                var codeExists = await _context.JewelFaults.AnyAsync(f => f.FaultCode == fault.FaultCode);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Fault code already exists"
                    });
                }

                // Validate valuation impact
                if (fault.AffectsValuation && (fault.ValuationImpactPercentage == null ||
                    fault.ValuationImpactPercentage < 0 || fault.ValuationImpactPercentage > 100))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Valuation impact percentage must be between 0 and 100 when affects valuation is true"
                    });
                }

                fault.Id = Guid.NewGuid();
                fault.CreatedDate = DateTime.UtcNow;
                fault.UpdatedDate = DateTime.UtcNow;
                fault.FaultCode = fault.FaultCode.ToUpper();
                fault.FaultType = fault.FaultType.ToUpper();
                fault.Severity = fault.Severity.ToUpper();

                _context.JewelFaults.Add(fault);
                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(fault).Reference(f => f.ItemGroup).LoadAsync();

                return CreatedAtAction(nameof(GetJewelFault), new { id = fault.Id }, new
                {
                    success = true,
                    message = "Jewel fault created successfully",
                    data = new
                    {
                        fault.Id,
                        fault.FaultCode,
                        fault.FaultName,
                        fault.FaultType,
                        fault.Description,
                        fault.Severity,
                        fault.AffectsValuation,
                        fault.ValuationImpactPercentage,
                        fault.ItemGroupId,
                        ItemGroupName = fault.ItemGroup?.GroupName,
                        fault.IsActive,
                        fault.CreatedDate,
                        fault.UpdatedDate,
                        fault.CreatedBy,
                        fault.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the jewel fault",
                    error = ex.Message
                });
            }
        }

        // PUT: api/jewelfaults/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateJewelFault(Guid id, JewelFault fault)
        {
            if (id != fault.Id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID mismatch"
                });
            }

            try
            {
                var existingFault = await _context.JewelFaults.FindAsync(id);
                if (existingFault == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Jewel fault not found"
                    });
                }

                // Validate item group exists
                var itemGroupExists = await _context.ItemGroups.AnyAsync(g => g.Id == fault.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid item group"
                    });
                }

                // Check if fault code already exists for another fault
                var codeExists = await _context.JewelFaults
                    .AnyAsync(f => f.FaultCode == fault.FaultCode && f.Id != id);
                if (codeExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Fault code already exists"
                    });
                }

                // Validate valuation impact
                if (fault.AffectsValuation && (fault.ValuationImpactPercentage == null ||
                    fault.ValuationImpactPercentage < 0 || fault.ValuationImpactPercentage > 100))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Valuation impact percentage must be between 0 and 100 when affects valuation is true"
                    });
                }

                existingFault.FaultCode = fault.FaultCode.ToUpper();
                existingFault.FaultName = fault.FaultName;
                existingFault.FaultType = fault.FaultType.ToUpper();
                existingFault.Description = fault.Description;
                existingFault.Severity = fault.Severity.ToUpper();
                existingFault.AffectsValuation = fault.AffectsValuation;
                existingFault.ValuationImpactPercentage = fault.ValuationImpactPercentage;
                existingFault.ItemGroupId = fault.ItemGroupId;
                existingFault.IsActive = fault.IsActive;
                existingFault.UpdatedDate = DateTime.UtcNow;
                existingFault.UpdatedBy = fault.UpdatedBy;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(existingFault).Reference(f => f.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = "Jewel fault updated successfully",
                    data = new
                    {
                        existingFault.Id,
                        existingFault.FaultCode,
                        existingFault.FaultName,
                        existingFault.FaultType,
                        existingFault.Description,
                        existingFault.Severity,
                        existingFault.AffectsValuation,
                        existingFault.ValuationImpactPercentage,
                        existingFault.ItemGroupId,
                        ItemGroupName = existingFault.ItemGroup?.GroupName,
                        existingFault.IsActive,
                        existingFault.CreatedDate,
                        existingFault.UpdatedDate,
                        existingFault.CreatedBy,
                        existingFault.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the jewel fault",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/jewelfaults/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteJewelFault(Guid id)
        {
            try
            {
                var fault = await _context.JewelFaults.FindAsync(id);
                if (fault == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Jewel fault not found"
                    });
                }

                _context.JewelFaults.Remove(fault);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Jewel fault deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the jewel fault",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/jewelfaults/{id}/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<object>> ToggleJewelFaultStatus(Guid id)
        {
            try
            {
                var fault = await _context.JewelFaults.FindAsync(id);
                if (fault == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Jewel fault not found"
                    });
                }

                fault.IsActive = !fault.IsActive;
                fault.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load the item group for response
                await _context.Entry(fault).Reference(f => f.ItemGroup).LoadAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Jewel fault {(fault.IsActive ? "activated" : "deactivated")} successfully",
                    data = new
                    {
                        fault.Id,
                        fault.FaultCode,
                        fault.FaultName,
                        fault.FaultType,
                        fault.Description,
                        fault.Severity,
                        fault.AffectsValuation,
                        fault.ValuationImpactPercentage,
                        fault.ItemGroupId,
                        ItemGroupName = fault.ItemGroup?.GroupName,
                        fault.IsActive,
                        fault.CreatedDate,
                        fault.UpdatedDate,
                        fault.CreatedBy,
                        fault.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while toggling jewel fault status",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/generate-code
        [HttpGet("generate-code")]
        public async Task<ActionResult<object>> GenerateFaultCode()
        {
            try
            {
                // Get the last fault code
                var lastFault = await _context.JewelFaults
                    .OrderByDescending(f => f.FaultCode)
                    .FirstOrDefaultAsync();

                string newCode;
                if (lastFault != null && lastFault.FaultCode.StartsWith("JF"))
                {
                    // Extract number and increment
                    var numberPart = lastFault.FaultCode.Substring(2);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        newCode = $"JF{(lastNumber + 1):D3}";
                    }
                    else
                    {
                        newCode = "JF001";
                    }
                }
                else
                {
                    newCode = "JF001";
                }

                return Ok(new
                {
                    success = true,
                    message = "Fault code generated successfully",
                    data = newCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating fault code",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/check-code/{code}
        [HttpGet("check-code/{code}")]
        public async Task<ActionResult<object>> CheckFaultCodeExists(string code)
        {
            try
            {
                var exists = await _context.JewelFaults.AnyAsync(f => f.FaultCode == code.ToUpper());

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
                    message = "An error occurred while checking fault code",
                    error = ex.Message
                });
            }
        }

        // GET: api/jewelfaults/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetJewelFaultStatistics()
        {
            try
            {
                var totalFaults = await _context.JewelFaults.CountAsync();
                var activeFaults = await _context.JewelFaults.CountAsync(f => f.IsActive);
                var inactiveFaults = totalFaults - activeFaults;

                var faultsByType = await _context.JewelFaults
                    .GroupBy(f => f.FaultType)
                    .Select(g => new
                    {
                        FaultType = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var faultsBySeverity = await _context.JewelFaults
                    .GroupBy(f => f.Severity)
                    .Select(g => new
                    {
                        Severity = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var affectsValuationCount = await _context.JewelFaults
                    .CountAsync(f => f.AffectsValuation);

                return Ok(new
                {
                    success = true,
                    message = "Statistics retrieved successfully",
                    data = new
                    {
                        totalFaults,
                        activeFaults,
                        inactiveFaults,
                        faultsByType,
                        faultsBySeverity,
                        affectsValuationCount
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