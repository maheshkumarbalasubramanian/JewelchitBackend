using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoldLoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GoldLoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/goldloans
        [HttpGet]
        public async Task<ActionResult<object>> GetGoldLoans(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? areaId = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.GoldLoans
                    .Include(gl => gl.Area)
                    .Include(gl => gl.Customer)
                    .Include(gl => gl.Scheme)
                    .Include(gl => gl.ItemGroup)
                    .Include(gl => gl.PledgedItems)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(gl =>
                        gl.LoanNumber.Contains(searchTerm) ||
                        gl.RefNumber!.Contains(searchTerm) ||
                        gl.Customer!.CustomerName.Contains(searchTerm));
                }

                if (areaId.HasValue)
                {
                    query = query.Where(gl => gl.AreaId == areaId.Value);
                }

                if (customerId.HasValue)
                {
                    query = query.Where(gl => gl.CustomerId == customerId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(gl => gl.Status == status);
                }

                var total = await query.CountAsync();

                var loans = await query
                    .OrderByDescending(gl => gl.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(gl => new
                    {
                        gl.Id,
                        gl.LoanNumber,
                        gl.RefNumber,
                        gl.LoanDate,
                        gl.MaturityDate,
                        gl.AreaId,
                        AreaName = gl.Area!.AreaName,
                        gl.CustomerId,
                        CustomerName = gl.Customer!.CustomerName,
                        gl.SchemeId,
                        SchemeName = gl.Scheme!.SchemeName,
                        gl.ItemGroupId,
                        ItemGroupName = gl.ItemGroup!.GroupName,
                        gl.LoanAmount,
                        gl.NetPayable,
                        gl.TotalNetWeight,
                        gl.Status,
                        gl.CreatedDate,
                        ItemsCount = gl.PledgedItems.Count
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Gold loans retrieved successfully",
                    data = new
                    {
                        loans,
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
                    message = "Error retrieving gold loans",
                    error = ex.Message
                });
            }
        }

        // GET: api/goldloans/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetGoldLoan(Guid id)
        {
            try
            {
                var loan = await _context.GoldLoans
                    .Include(gl => gl.Area)
                    .Include(gl => gl.Customer)
                    .Include(gl => gl.Scheme)
                    .Include(gl => gl.ItemGroup)
                    .Include(gl => gl.PledgedItems)
                        .ThenInclude(pi => pi.ItemType)
                    .Include(gl => gl.PledgedItems)
                        .ThenInclude(pi => pi.Purity)
                    .FirstOrDefaultAsync(gl => gl.Id == id);

                if (loan == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Gold loan not found"
                    });
                }

                // Return structured data
                var response = new
                {
                    id = loan.Id,
                    series = loan.Series,
                    loanNumber = loan.LoanNumber,
                    refNumber = loan.RefNumber,
                    loanDate = loan.LoanDate,
                    maturityDate = loan.MaturityDate,
                    areaId = loan.AreaId,
                    customerId = loan.CustomerId,
                    customerImage = loan.CustomerImage,
                    schemeId = loan.SchemeId,
                    itemGroupId = loan.ItemGroupId,
                    loanAmount = loan.LoanAmount,
                    interestRate = loan.InterestRate,
                    interestAmount = loan.InterestAmount,
                    advanceMonths = loan.AdvanceMonths,
                    advanceInterestAmount = loan.AdvanceInterestAmount,
                    processingFeePercent = loan.ProcessingFeePercent,
                    processingFeeAmount = loan.ProcessingFeeAmount,
                    netPayable = loan.NetPayable,
                    totalQty = loan.TotalQty,
                    totalGrossWeight = loan.TotalGrossWeight,
                    totalStoneWeight = loan.TotalStoneWeight,
                    totalNetWeight = loan.TotalNetWeight,
                    totalCalculatedValue = loan.TotalCalculatedValue,
                    totalMaximumValue = loan.TotalMaximumValue,
                    dueMonths = loan.DueMonths,
                    remarks = loan.Remarks,
                    status = loan.Status,
                    customer = loan.Customer != null ? new
                    {
                        id = loan.Customer.Id,
                        customerName = loan.Customer.CustomerName,
                        customerCode = loan.Customer.CustomerCode
                    } : null,
                    pledgedItems = loan.PledgedItems.Select(pi => new
                    {
                        id = pi.Id,
                        itemTypeId = pi.ItemTypeId,
                        itemName = pi.ItemName ?? "",
                        purityId = pi.PurityId,
                        goldRate = pi.GoldRate,
                        qty = pi.Qty,
                        grossWeight = pi.GrossWeight,
                        stoneWeight = pi.StoneWeight,
                        netWeight = pi.NetWeight,
                        calculatedValue = pi.CalculatedValue,
                        maximumValue = pi.MaximumValue,
                        remarks = pi.Remarks,
                        jewelFault = pi.JewelFault,
                        huid = pi.Huid,
                        hallmarkPurity = pi.HallmarkPurity,
                        hallmarkGrossWeight = pi.HallmarkGrossWeight,
                        hallmarkNetWeight = pi.HallmarkNetWeight,
                        images = pi.Images
                    }).ToList()
                };

                return Ok(new
                {
                    success = true,
                    message = "Gold loan retrieved successfully",
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving gold loan",
                    error = ex.Message
                });
            }
        }

        // GET: api/goldloans/customer/{customerId}/history
        [HttpGet("customer/{customerId}/history")]
        public async Task<ActionResult<object>> GetCustomerLoanHistory(Guid customerId)
        {
            try
            {
                //var customer = await _context.Customers.FindAsync(customerId);

                var customer = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.Id == customerId)
                    .Select(c => new
                    {
                        c.Id,
                        c.CustomerCode,
                        c.CustomerName,
                        c.RelationName,
                        c.RelationshipName,
                        c.ProfileImage,
                        c.Mobile,
                        Address1 = c.AddressInfo != null ? c.AddressInfo.Address1 : null
                    })
                    .FirstOrDefaultAsync();


                if (customer == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }

                var loans = await _context.GoldLoans
                    .Where(gl => gl.CustomerId == customerId)
                    .OrderByDescending(gl => gl.LoanDate)
                    .Select(gl => new
                    {
                        gl.Id,
                        gl.LoanNumber,
                        gl.LoanDate,
                        gl.LoanAmount,
                        gl.TotalNetWeight,
                        gl.Status,
                        Duration = gl.DueMonths + " months"
                    })
                    .ToListAsync();

                var loanCounts = new
                {
                    live = loans.Count(l => l.Status == "Open"),
                    closed = loans.Count(l => l.Status == "Closed"),
                    matured = loans.Count(l => l.Status == "Matured"),
                    auctioned = loans.Count(l => l.Status == "Auctioned")
                };

                return Ok(new
                {
                    success = true,
                    message = "Customer loan history retrieved successfully",
                    data = new
                    {
                        customerName = customer.CustomerName,
                        customerCode = customer.CustomerCode,
                        Mobile = customer.Mobile??"",
                        Address = customer.Address1,
                        relationName = customer.RelationName,
                        relationshipName = customer.RelationshipName,
                        ProfileImage = customer.ProfileImage??"",
                        loanCounts,
                        loans
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving customer loan history",
                    error = ex.Message
                });
            }
        }

        // GET: api/goldloans/generate-loan-number
        [HttpGet("generate-loan-number")]
        public async Task<ActionResult<object>> GenerateLoanNumber()
        {
            try
            {
                var today = DateTime.UtcNow;
                var prefix = $"GL{today:yyMMdd}";

                var lastLoan = await _context.GoldLoans
                    .Where(gl => gl.LoanNumber.StartsWith(prefix))
                    .OrderByDescending(gl => gl.LoanNumber)
                    .FirstOrDefaultAsync();

                string loanNumber;
                if (lastLoan != null && lastLoan.LoanNumber.Length > prefix.Length)
                {
                    var lastSequence = lastLoan.LoanNumber.Substring(prefix.Length);
                    if (int.TryParse(lastSequence, out int sequence))
                    {
                        loanNumber = $"{prefix}{(sequence + 1):D4}";
                    }
                    else
                    {
                        loanNumber = $"{prefix}0001";
                    }
                }
                else
                {
                    loanNumber = $"{prefix}0001";
                }

                return Ok(new
                {
                    success = true,
                    message = "Loan number generated successfully",
                    data = loanNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating loan number",
                    error = ex.Message
                });
            }
        }

        // POST: api/goldloans
        [HttpPost]
        public async Task<ActionResult<object>> CreateGoldLoan([FromBody] GoldLoan goldLoan)
        {
            try
            {
                // Validate relationships
                var areaExists = await _context.Areas.AnyAsync(a => a.Id == goldLoan.AreaId);
                if (!areaExists)
                {
                    return BadRequest(new { success = false, message = "Invalid area ID" });
                }

                var customerExists = await _context.Customers.AnyAsync(c => c.Id == goldLoan.CustomerId);
                if (!customerExists)
                {
                    return BadRequest(new { success = false, message = "Invalid customer ID" });
                }

                var schemeExists = await _context.Schemes.AnyAsync(s => s.Id == goldLoan.SchemeId);
                if (!schemeExists)
                {
                    return BadRequest(new { success = false, message = "Invalid scheme ID" });
                }

                var itemGroupExists = await _context.ItemGroups.AnyAsync(ig => ig.Id == goldLoan.ItemGroupId);
                if (!itemGroupExists)
                {
                    return BadRequest(new { success = false, message = "Invalid item group ID" });
                }

                // Generate loan number if not provided
                if (string.IsNullOrEmpty(goldLoan.LoanNumber))
                {
                    var loanNumberResponse = await GenerateLoanNumber();
                    if (loanNumberResponse.Value is OkObjectResult okResult && okResult.Value != null)
                    {
                        var result = okResult.Value as dynamic;
                        goldLoan.LoanNumber = result?.data ?? string.Empty;
                    }
                }

                goldLoan.Id = Guid.NewGuid();
                goldLoan.CreatedDate = DateTime.UtcNow;
                goldLoan.Status = "Open";

                // Set IDs for pledged items
                foreach (var item in goldLoan.PledgedItems)
                {
                    item.Id = Guid.NewGuid();
                    item.GoldLoanId = goldLoan.Id;
                    item.CreatedDate = DateTime.UtcNow;
                }

                _context.GoldLoans.Add(goldLoan);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetGoldLoan), new { id = goldLoan.Id }, new
                {
                    success = true,
                    message = "Gold loan created successfully",
                    data = new { goldLoan.Id, goldLoan.LoanNumber }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error creating gold loan",
                    error = ex.Message
                });
            }
        }

        // PUT: api/goldloans/{id}
        // PUT: api/goldloans/{id}

        // PUT: api/goldloans/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateGoldLoan(Guid id, [FromBody] GoldLoan goldLoan)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (id != goldLoan.Id)
                {
                    return BadRequest(new { success = false, message = "ID mismatch" });
                }

                // Step 1: Delete existing items - use the actual table name from your database
                var deleteSql = "DELETE FROM pledged_items WHERE gold_loan_id = @p0";
                await _context.Database.ExecuteSqlRawAsync(deleteSql, id);

                // Step 2: Update the loan
                var existingLoan = await _context.GoldLoans.FindAsync(id);
                if (existingLoan == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { success = false, message = "Gold loan not found" });
                }

                // Update loan properties
                existingLoan.Series = goldLoan.Series;
                existingLoan.RefNumber = goldLoan.RefNumber;
                existingLoan.LoanDate = goldLoan.LoanDate;
                existingLoan.MaturityDate = goldLoan.MaturityDate;
                existingLoan.AreaId = goldLoan.AreaId;
                existingLoan.CustomerId = goldLoan.CustomerId;
                existingLoan.CustomerImage = goldLoan.CustomerImage;
                existingLoan.SchemeId = goldLoan.SchemeId;
                existingLoan.ItemGroupId = goldLoan.ItemGroupId;
                existingLoan.LoanAmount = goldLoan.LoanAmount;
                existingLoan.InterestRate = goldLoan.InterestRate;
                existingLoan.InterestAmount = goldLoan.InterestAmount;
                existingLoan.AdvanceMonths = goldLoan.AdvanceMonths;
                existingLoan.AdvanceInterestAmount = goldLoan.AdvanceInterestAmount;
                existingLoan.ProcessingFeePercent = goldLoan.ProcessingFeePercent;
                existingLoan.ProcessingFeeAmount = goldLoan.ProcessingFeeAmount;
                existingLoan.NetPayable = goldLoan.NetPayable;
                existingLoan.TotalQty = goldLoan.TotalQty;
                existingLoan.TotalGrossWeight = goldLoan.TotalGrossWeight;
                existingLoan.TotalStoneWeight = goldLoan.TotalStoneWeight;
                existingLoan.TotalNetWeight = goldLoan.TotalNetWeight;
                existingLoan.TotalCalculatedValue = goldLoan.TotalCalculatedValue;
                existingLoan.TotalMaximumValue = goldLoan.TotalMaximumValue;
                existingLoan.DueMonths = goldLoan.DueMonths;
                existingLoan.Remarks = goldLoan.Remarks;
                existingLoan.UpdatedDate = DateTime.UtcNow;
                existingLoan.UpdatedBy = goldLoan.UpdatedBy;

                // Step 3: Add new items
                foreach (var item in goldLoan.PledgedItems)
                {
                    var newItem = new PledgedItem
                    {
                        Id = Guid.NewGuid(),
                        GoldLoanId = id,
                        ItemTypeId = item.ItemTypeId,
                        ItemName = item.ItemName,
                        PurityId = item.PurityId,
                        GoldRate = item.GoldRate,
                        Qty = item.Qty,
                        GrossWeight = item.GrossWeight,
                        StoneWeight = item.StoneWeight,
                        NetWeight = item.NetWeight,
                        CalculatedValue = item.CalculatedValue,
                        MaximumValue = item.MaximumValue,
                        Remarks = item.Remarks,
                        JewelFault = item.JewelFault,
                        Huid = item.Huid,
                        HallmarkPurity = item.HallmarkPurity,
                        HallmarkGrossWeight = item.HallmarkGrossWeight,
                        HallmarkNetWeight = item.HallmarkNetWeight,
                        Images = item.Images,
                        CreatedDate = DateTime.UtcNow

                    };
                    _context.PledgedItems.Add(newItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Gold loan updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating gold loan",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        // DELETE: api/goldloans/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteGoldLoan(Guid id)
        {
            try
            {
                var loan = await _context.GoldLoans
                    .Include(gl => gl.PledgedItems)
                    .FirstOrDefaultAsync(gl => gl.Id == id);

                if (loan == null)
                {
                    return NotFound(new { success = false, message = "Gold loan not found" });
                }

                _context.GoldLoans.Remove(loan);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Gold loan deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting gold loan",
                    error = ex.Message
                });
            }
        }

        // PATCH: api/goldloans/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<object>> UpdateLoanStatus(Guid id, [FromBody] string status)
        {
            try
            {
                var loan = await _context.GoldLoans.FindAsync(id);
                if (loan == null)
                {
                    return NotFound(new { success = false, message = "Gold loan not found" });
                }

                loan.Status = status;
                loan.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Loan status updated to {status}",
                    data = new { loan.Status }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating loan status",
                    error = ex.Message
                });
            }
        }
    }
}