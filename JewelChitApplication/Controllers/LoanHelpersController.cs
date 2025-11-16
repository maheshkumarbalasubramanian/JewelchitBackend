using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanHelpersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoanHelpersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("calculate-interest-fees")]
        public IActionResult CalculateInterestAndFees([FromBody] InterestCalculationRequest request)
        {
            try
            {
                decimal interestAmount = 0;
                decimal advanceInterestAmount = 0;

                if (request.CalculationMethod == "Simple")
                {
                    // Calculate total interest for entire tenure
                    interestAmount = (request.LoanAmount * request.Roi * request.DueMonths) / (100);

                    // Calculate advance interest
                    advanceInterestAmount = (request.LoanAmount * request.Roi * request.AdvanceMonths) / (100);
                }
                else if (request.CalculationMethod == "Reducing")
                {
                    // Reducing balance calculation
                    decimal monthlyRate = request.Roi / (12 * 100);
                    interestAmount = request.LoanAmount * monthlyRate * request.DueMonths;
                    advanceInterestAmount = request.LoanAmount * monthlyRate * request.AdvanceMonths;
                }

                decimal processingFeeAmount = (request.LoanAmount * request.ProcessingFeePercent) / 100;
                decimal netPayable = request.LoanAmount - advanceInterestAmount - processingFeeAmount;

                var response = new
                {
                    interestAmount = Math.Round(interestAmount, 2),
                    processingFeeAmount = Math.Round(processingFeeAmount, 2),
                    advanceInterestAmount = Math.Round(advanceInterestAmount, 2),
                    netPayable = Math.Round(netPayable, 2)
                };

                return Ok(new { success = true, message = "Interest and fees calculated successfully", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class InterestCalculationRequest
        {
            public decimal LoanAmount { get; set; }
            public decimal Roi { get; set; }
            public string CalculationMethod { get; set; }
            public int DueMonths { get; set; }
            public int AdvanceMonths { get; set; }
            public decimal ProcessingFeePercent { get; set; }
        }



        // GET: api/loanhelpers/customers-by-area/{areaId}
        [HttpGet("customers-by-area/{areaId}")]
        public async Task<ActionResult<object>> GetCustomersByArea(Guid areaId)
        {
            try
            {
                var customers = await _context.Customers
                    //.Where(c => c.AreaId == areaId && c.IsActive == true)
                    .OrderBy(c => c.CustomerName)
                    .Select(c => new
                    {
                        c.Id,
                        c.CustomerCode,
                        c.CustomerName,
                        c.RelationName,
                        c.RelationshipName,
                        c.Mobile,
                        c.AddressInfo.Address1
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Customers retrieved successfully",
                    data = customers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving customers",
                    error = ex.Message
                });
            }
        }

        // GET: api/loanhelpers/itemgroups-by-area/{areaId}
        [HttpGet("itemgroups-by-area/{areaId}")]
        public async Task<ActionResult<object>> GetItemGroupsByArea(Guid areaId)
        {
            try
            {
                var itemGroups = await _context.ItemGroups
                    .Where(ig => ig.AreaId == areaId && ig.IsActive == true)
                    .OrderBy(ig => ig.GroupName)
                    .Select(ig => new
                    {
                        ig.Id,
                        ig.GroupCode,
                        ig.GroupName
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Item groups retrieved successfully",
                    data = itemGroups
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving item groups",
                    error = ex.Message
                });
            }
        }

        // GET: api/loanhelpers/schemes-by-itemgroup/{itemGroupId}
        [HttpGet("schemes-by-itemgroup/{itemGroupId}")]
        public async Task<ActionResult<object>> GetSchemesByItemGroup(Guid itemGroupId)
        {
            try
            {
                var schemes = await _context.Schemes
                    .Where(s => s.ItemGroupId == itemGroupId && s.IsActive == true)
                    .OrderBy(s => s.SchemeName)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.Roi,
                        s.CalculationMethod,
                        s.ProcessingFeePercent,
                        s.AdvanceMonth,
                        s.MinCalcDays,
                        s.GraceDays,
                        s.MinLoanValue,
                        s.MaxLoanValue,
                        s.ReductionPercent
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Schemes retrieved successfully",
                    data = schemes
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

        // GET: api/loanhelpers/itemtypes-by-itemgroup/{itemGroupId}
        [HttpGet("itemtypes-by-itemgroup/{itemGroupId}")]
        public async Task<ActionResult<object>> GetItemTypesByItemGroup(Guid itemGroupId)
        {
            try
            {
                var itemTypes = await _context.ItemTypes
                    .Where(it => it.ItemGroupId == itemGroupId && it.IsActive == true)
                    .OrderBy(it => it.ItemName)
                    .Select(it => new
                    {
                        it.Id,
                        it.ItemCode,
                        it.ItemName,
                        it.ItemNameTamil
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
                    message = "Error retrieving item types",
                    error = ex.Message
                });
            }
        }

        // GET: api/loanhelpers/purities-by-itemgroup/{itemGroupId}
        [HttpGet("purities-by-itemgroup/{itemGroupId}")]
        public async Task<ActionResult<object>> GetPuritiesByItemGroup(Guid itemGroupId)
        {
            try
            {
                // Get purities associated with the item group
                var purities = await _context.Purities
                    .Where(p => p.ItemGroupId == itemGroupId && p.IsActive == true)
                    .OrderBy(p => p.PurityName)
                    .Select(p => new
                    {
                        p.Id,
                        p.PurityName,
                        p.PurityPercentage,
                        p.Karat
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
                    message = "Error retrieving purities",
                    error = ex.Message
                });
            }
        }
        // GET: api/loanhelpers/purities-by-itemgroup/{itemGroupId}
        [HttpGet("faults-by-itemgroup/{itemGroupId}")]
        public async Task<ActionResult<object>> GetFaultsByItemGroup(Guid itemGroupId)
        {
            try
            {
                // Get purities associated with the item group
                var faults = await _context.JewelFaults
                    .Where(p => p.ItemGroupId == itemGroupId && p.IsActive == true)
                    .OrderBy(p => p.FaultName)
                    .Select(p => new
                    {
                        p.Id,
                        p.FaultName,
                        p.FaultCode,
                        p.AffectsValuation,
                        p.ValuationImpactPercentage
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Faults retrieved successfully",
                    data = faults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving purities",
                    error = ex.Message
                });
            }
        }


        // GET: api/loanhelpers/scheme-details/{schemeId}
        [HttpGet("scheme-details/{schemeId}")]
        public async Task<ActionResult<object>> GetSchemeDetails(Guid schemeId)
        {
            try
            {
                var scheme = await _context.Schemes
                    .Where(s => s.Id == schemeId)
                    .Select(s => new
                    {
                        s.Id,
                        s.SchemeCode,
                        s.SchemeName,
                        s.Roi,
                        s.CalculationMethod,
                        s.IsStdRoi,
                        s.CalculationBased,
                        s.CustomizedStyle,
                        s.ProcessingFeeSlab,
                        s.ProcessingFeePercent,
                        s.MinCalcDays,
                        s.GraceDays,
                        s.AdvanceMonth,
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
                        s.InterestPercentAfterValidity
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
                    message = "Scheme details retrieved successfully",
                    data = scheme
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving scheme details",
                    error = ex.Message
                });
            }
        }

        // GET: api/loanhelpers/active-areas
        [HttpGet("active-areas")]
        public async Task<ActionResult<object>> GetActiveAreas()
        {
            try
            {
                var areas = await _context.Areas
                    .Where(a => a.IsActive == true)
                    .OrderBy(a => a.AreaName)
                    .Select(a => new
                    {
                        a.Id,
                        a.AreaCode,
                        a.AreaName
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Active areas retrieved successfully",
                    data = areas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving areas",
                    error = ex.Message
                });
            }
        }

        // POST: api/loanhelpers/calculate-loan
        [HttpPost("calculate-loan")]
        public ActionResult<object> CalculateLoan([FromBody] LoanCalculationRequest request)
        {
            try
            {
                // Validate inputs
                if (request.MaximumValue <= 0 || request.Roi <= 0 || request.DueMonths <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid calculation parameters"
                    });
                }

                // Calculate loan amount (typically 75% of maximum value)
                decimal ltvRatio = 100; // Can be fetched from scheme
                decimal loanAmount = request.MaximumValue * (ltvRatio / 100);

                // Calculate processing fee
                decimal processingFeeAmount = (loanAmount * request.ProcessingFeePercent) / 100;

                // Calculate interest based on method
                decimal interestAmount = 0;

                if (request.CalculationMethod == "Simple")
                {
                    // Simple Interest: (P × R × T) / 100
                    interestAmount = (loanAmount * request.Roi * request.DueMonths) / (12 * 100);
                }
                else if (request.CalculationMethod == "Compound")
                {
                    // Compound Interest: P[(1 + r/n)^(nt) - 1]
                    decimal monthlyRate = request.Roi / 1200; // Convert annual % to monthly decimal
                    interestAmount = loanAmount * (decimal)(Math.Pow((double)(1 + monthlyRate), request.DueMonths) - 1);
                }
                else if (request.CalculationMethod == "Emi")
                {
                    // EMI: [P × r × (1+r)^n] / [(1+r)^n-1]
                    decimal monthlyRate = request.Roi / 1200;
                    decimal emi = loanAmount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), request.DueMonths)
                                / (decimal)(Math.Pow((double)(1 + monthlyRate), request.DueMonths) - 1);
                    interestAmount = (emi * request.DueMonths) - loanAmount;
                }
                else
                {
                    // Default to simple interest
                    interestAmount = (loanAmount * request.Roi * request.DueMonths) / (12 * 100);
                }

                // Calculate advance interest
                decimal advanceInterestAmount = (loanAmount * request.Roi * request.AdvanceMonths) / (12 * 100);

                // Calculate net payable
                decimal netPayable = loanAmount - advanceInterestAmount - processingFeeAmount;

                return Ok(new
                {
                    success = true,
                    message = "Loan calculation completed",
                    data = new
                    {
                        loanAmount = Math.Round(loanAmount, 2),
                        interestAmount = Math.Round(interestAmount, 2),
                        processingFeeAmount = Math.Round(processingFeeAmount, 2),
                        advanceInterestAmount = Math.Round(advanceInterestAmount, 2),
                        netPayable = Math.Round(netPayable, 2),
                        totalRepayable = Math.Round(loanAmount + interestAmount, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error calculating loan",
                    error = ex.Message
                });
            }
        }
    }

    // Request model for loan calculation
    public class LoanCalculationRequest
    {
        public decimal MaximumValue { get; set; }
        public decimal Roi { get; set; }
        public string CalculationMethod { get; set; } = "Simple";
        public int DueMonths { get; set; }
        public int AdvanceMonths { get; set; }
        public decimal ProcessingFeePercent { get; set; }
    }
}