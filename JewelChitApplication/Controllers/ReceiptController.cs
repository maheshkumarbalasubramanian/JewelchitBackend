using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/receipts
        [HttpGet]
        public async Task<ActionResult<object>> GetReceipts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? loanId = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Receipts
                    .Include(r => r.Customer)
                    .Include(r => r.GoldLoan)
                    .Include(r => r.InterestStatements)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(r =>
                        r.ReceiptNumber.Contains(searchTerm) ||
                        r.LoanNumber.Contains(searchTerm) ||
                        r.Customer!.CustomerName.Contains(searchTerm));
                }

                if (loanId.HasValue)
                {
                    query = query.Where(r => r.GoldLoanId == loanId.Value);
                }

                if (customerId.HasValue)
                {
                    query = query.Where(r => r.CustomerId == customerId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status);
                }

                var total = await query.CountAsync();

                var receipts = await query
                    .OrderByDescending(r => r.ReceiptDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReceiptNumber,
                        r.ReceiptDate,
                        r.TillDate,
                        r.LoanNumber,
                        CustomerName = r.Customer!.CustomerName,
                        r.CustomerCode,
                        r.Customer.ProfileImage,
                        r.GoldLoan.CustomerImage,
                        r.PaymentType,
                        r.PrincipalAmount,
                        r.InterestAmount,
                        r.NetPayable,
                        r.OutstandingPrincipal,
                        r.Status,
                        r.CreatedDate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Receipts retrieved successfully",
                    data = new { receipts, total, page, pageSize }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving receipts",
                    error = ex.Message
                });
            }
        }

        // GET: api/receipts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetReceipt(Guid id)
        {
            try
            {
                var receipt = await _context.Receipts
                    .Include(r => r.Customer)
                    .Include(r => r.GoldLoan)
                        .ThenInclude(gl => gl.PledgedItems)
                    .Include(r => r.GoldLoan)
                        .ThenInclude(gl => gl.Scheme)
                    .Include(r => r.GoldLoan)
                        .ThenInclude(gl => gl.ItemGroup)
                    //.Include(r => r.InterestStatements)
                    //.Include(r => r.PaymentModes)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (receipt == null)
                {
                    return NotFound(new { success = false, message = "Receipt not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Receipt retrieved successfully",
                    data = receipt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving receipt",
                    error = ex.Message
                });
            }
        }

        // GET: api/receipts/loan/{loanId}/calculate-interest
        // GET: api/receipts/loan/{loanId}/calculate-interest
        // GET: api/receipts/loan/{loanId}/calculate-interest
        [HttpGet("loan/{loanId}/calculate-interest")]
        public async Task<ActionResult<object>> CalculateInterest(Guid loanId, [FromQuery] DateTime tillDate)
        {
            try
            {
                var loan = await _context.GoldLoans
                    .Include(gl => gl.Scheme)
                    .FirstOrDefaultAsync(gl => gl.Id == loanId);

                if (loan == null)
                {
                    return NotFound(new { success = false, message = "Loan not found" });
                }

                // Get last receipt
                var lastReceipt = await _context.Receipts
                    .Where(r => r.GoldLoanId == loanId && r.Status == "Completed")
                    .OrderByDescending(r => r.TillDate)  // Important: Order by TillDate, not ReceiptDate
                    .ThenByDescending(r => r.CreatedDate)
                    .FirstOrDefaultAsync();

                // Get all historical receipts for interest statement
                var allReceipts = await _context.Receipts
                    .Where(r => r.GoldLoanId == loanId && r.Status == "Completed")
                    .Include(r => r.InterestStatements)
                    .OrderBy(r => r.TillDate)
                    .ThenBy(r => r.CreatedDate)
                    .ToListAsync();

                // Calculate advance interest end date
                var advanceMonths = loan.Scheme?.AdvanceMonth ?? 0;
                var advanceInterestEndDate = loan.LoanDate.AddMonths(advanceMonths);

                // Determine the actual start date for interest calculation
                DateTime interestAccrualStartDate;

                if (lastReceipt != null)
                {
                    // Start from the last TillDate (last date interest was paid up to)
                    interestAccrualStartDate = lastReceipt.TillDate;
                }
                else
                {
                    // For first payment, start after advance interest period
                    interestAccrualStartDate = advanceInterestEndDate;
                }

                // Apply grace days only if this is the first calculation after loan or last payment
                var graceDays = loan.Scheme?.GraceDays ?? 0;
                var interestStartDate = interestAccrualStartDate.AddDays(graceDays);

                // Calculate actual days from last paid date to tillDate
                var totalDaysDue = (tillDate - interestStartDate).Days;

                // If still within advance period or grace period
                if (tillDate <= advanceInterestEndDate && lastReceipt == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Still within advance interest period",
                        data = new
                        {
                            loanDate = loan.LoanDate,
                            advanceInterestEndDate,
                            interestAccrualStartDate,
                            interestStartDate,
                            tillDate,
                            totalDaysDue = 0,
                            graceDays,
                            outstandingPrincipal = loan.LoanAmount,
                            totalInterestDue = 0.00m,
                            interestStatements = GenerateCompleteInterestStatement(loan, allReceipts, interestAccrualStartDate, tillDate),
                            note = $"Advance interest already collected for {advanceMonths} month(s)"
                        }
                    });
                }

                // If within grace period or no days due
                if (totalDaysDue <= 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = graceDays > 0 && totalDaysDue < 0
                            ? "Within grace period - no interest due yet"
                            : "Interest already paid up to this date",
                        data = new
                        {
                            loanDate = loan.LoanDate,
                            interestAccrualStartDate,
                            interestStartDate,
                            tillDate,
                            graceDays,
                            totalDaysDue = Math.Max(0, (tillDate - interestAccrualStartDate).Days),
                            outstandingPrincipal = lastReceipt?.OutstandingPrincipal ?? loan.LoanAmount,
                            totalInterestDue = 0.00m,
                            interestStatements = GenerateCompleteInterestStatement(loan, allReceipts, interestAccrualStartDate, tillDate),
                            lastReceiptDate = lastReceipt?.ReceiptDate,
                            lastReceiptNumber = lastReceipt?.ReceiptNumber,
                            lastPaidUpTo = lastReceipt?.TillDate
                        }
                    });
                }

                // Calculate total interest due
                decimal outstandingPrincipal = lastReceipt?.OutstandingPrincipal ?? loan.LoanAmount;
                var calculationMethod = loan.Scheme?.CalculationMethod ?? "Simple";

                // Calculate interest for total days due (including any unpaid periods)
                decimal totalInterestDue = CalculateInterestAmount(
                    outstandingPrincipal,
                    loan.InterestRate,
                    totalDaysDue,
                    calculationMethod
                );

                var statements = GenerateCompleteInterestStatement(loan, allReceipts, interestAccrualStartDate, tillDate);

                return Ok(new
                {
                    success = true,
                    message = "Interest calculated successfully",
                    data = new
                    {
                        loanDate = loan.LoanDate,
                        advanceInterestEndDate,
                        lastPaidUpTo = lastReceipt?.TillDate,
                        interestAccrualStartDate,
                        interestStartDate,
                        tillDate,
                        graceDays,
                        totalDaysDue,  // Total days from last paid date to now
                        outstandingPrincipal,
                        totalInterestDue = Math.Round(totalInterestDue, 2),  // Total interest accumulated
                        interestStatements = statements,
                        lastReceiptDate = lastReceipt?.ReceiptDate,
                        lastReceiptNumber = lastReceipt?.ReceiptNumber,
                        note = totalDaysDue > 30 ? $"Includes {Math.Round(totalDaysDue / 30.0, 1)} months of accumulated interest" : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error calculating interest",
                    error = ex.Message
                });
            }
        }

        // Helper method to calculate interest
        private decimal CalculateInterestAmount(decimal principal, decimal interestRate, int days, string calculationMethod)
        {
            decimal interestAmount = 0;

            if (calculationMethod == "Simple" || calculationMethod == "Monthly")
            {
                var months = days / 30.0m;
                interestAmount = (principal * interestRate * months) / 100;
            }
            else if (calculationMethod == "Daily")
            {
                interestAmount = (principal * interestRate * days) / (365 * 100);
            }
            else if (calculationMethod == "Compound")
            {
                var months = days / 30.0m;
                var monthlyRate = interestRate / 100;
                interestAmount = principal * (decimal)(Math.Pow((double)(1 + monthlyRate), (double)months) - 1);
            }
            else
            {
                var months = days / 30.0m;
                interestAmount = (principal * interestRate * months) / 100;
            }

            return interestAmount;
        }
        // PUT: api/receipts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateReceipt(Guid id, [FromBody] Receipt receipt)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if receipt exists
                var existingReceipt = await _context.Receipts
                    .Include(r => r.InterestStatements)
                    .Include(r => r.PaymentModes)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (existingReceipt == null)
                {
                    return NotFound(new { success = false, message = "Receipt not found" });
                }

                // Check if receipt is already cancelled
                if (existingReceipt.Status == "Cancelled")
                {
                    return BadRequest(new { success = false, message = "Cannot update a cancelled receipt" });
                }

                // Validate loan exists
                var loan = await _context.GoldLoans.FindAsync(receipt.GoldLoanId);
                if (loan == null)
                {
                    return BadRequest(new { success = false, message = "Invalid loan ID" });
                }

                // Remove old interest statements and payment modes
                //_context.InterestStatements.RemoveRange(existingReceipt.InterestStatements);
                //_context.PaymentModes.RemoveRange(existingReceipt.PaymentModes);

                // Update receipt properties
                existingReceipt.ReceiptDate = receipt.ReceiptDate;
                existingReceipt.TillDate = receipt.TillDate;
                existingReceipt.PaymentType = receipt.PaymentType;
                existingReceipt.PrincipalAmount = receipt.PrincipalAmount;
                existingReceipt.InterestAmount = receipt.InterestAmount;
                existingReceipt.OtherCredits = receipt.OtherCredits;
                existingReceipt.OtherDebits = receipt.OtherDebits;
                existingReceipt.DefaultAmount = receipt.DefaultAmount;
                existingReceipt.AddLess = receipt.AddLess;
                existingReceipt.NetPayable = receipt.NetPayable;
                existingReceipt.CalculatedInterest = receipt.CalculatedInterest;
                existingReceipt.OutstandingPrincipal = receipt.OutstandingPrincipal;
                existingReceipt.OutstandingInterest = receipt.OutstandingInterest;
                existingReceipt.Remarks = receipt.Remarks;
                existingReceipt.UpdatedDate = DateTime.UtcNow;

                // Add new interest statements
                foreach (var statement in receipt.InterestStatements)
                {
                    statement.Id = Guid.NewGuid();
                    statement.ReceiptId = existingReceipt.Id;
                    statement.GoldLoanId = receipt.GoldLoanId;
                    statement.CreatedDate = DateTime.UtcNow;
                    existingReceipt.InterestStatements.Add(statement);
                }

                // Add new payment modes
                foreach (var paymentMode in receipt.PaymentModes)
                {
                    paymentMode.Id = Guid.NewGuid();
                    paymentMode.ReceiptId = existingReceipt.Id;
                    paymentMode.CreatedDate = DateTime.UtcNow;
                    existingReceipt.PaymentModes.Add(paymentMode);
                }

                // Update loan status if full payment
                if (receipt.PaymentType == "full" && receipt.OutstandingPrincipal == 0)
                {
                    loan.Status = "Closed";
                    loan.UpdatedDate = DateTime.UtcNow;
                }
                else if (loan.Status == "Closed" && receipt.OutstandingPrincipal > 0)
                {
                    // If loan was closed but now has outstanding principal, reopen it
                    loan.Status = "Active";
                    loan.UpdatedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Receipt updated successfully",
                    data = new { existingReceipt.Id, existingReceipt.ReceiptNumber }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating receipt",
                    error = ex.Message
                });
            }
        }

        // New method to generate complete interest statement with history
        private List<object> GenerateCompleteInterestStatement(GoldLoan loan, List<Receipt> receipts, DateTime fromDate, DateTime toDate)
        {
            var statements = new List<object>();
            var currentPrincipal = loan.LoanAmount;

            // Add historical paid periods first
            foreach (var receipt in receipts)
            {
                if (receipt.InterestStatements != null && receipt.InterestStatements.Any())
                {
                    foreach (var stmt in receipt.InterestStatements.OrderBy(s => s.FromDate))
                    {
                        statements.Add(new
                        {
                            fromDate = stmt.FromDate.ToString("dd/MM/yyyy"),
                            toDate = stmt.ToDate.ToString("dd/MM/yyyy"),
                            duration = $"{stmt.DurationDays} Days",
                            intAccrued = Math.Round(stmt.InterestAccrued, 2),
                            totAccrued = Math.Round(stmt.TotalAccrued, 2),
                            intPaid = Math.Round(stmt.InterestPaid, 2),
                            principalPaid = Math.Round(stmt.PrincipalPaid, 2),
                            addedPrincipal = Math.Round(stmt.AddedPrincipal, 2),
                            adjPrincipal = Math.Round(stmt.AdjustedPrincipal, 2),
                            newPrincipal = Math.Round(stmt.NewPrincipal, 2),
                            isPaid = true
                        });
                    }
                    currentPrincipal = receipt.OutstandingPrincipal;
                }
            }

            // Add current unpaid period if there are days to calculate
            if (fromDate < toDate)
            {
                var currentDate = fromDate;
                var totalAccrued = statements.Any() ?
                    statements.Cast<dynamic>().Sum(s => (decimal)s.totAccrued) : 0m;

                while (currentDate < toDate)
                {
                    var nextDate = currentDate.AddDays(30) > toDate ? toDate : currentDate.AddDays(30);
                    var days = (nextDate - currentDate).Days;

                    var interestAccrued = (currentPrincipal * loan.InterestRate * days) / (30 * 100);
                    totalAccrued += interestAccrued;

                    statements.Add(new
                    {
                        fromDate = currentDate.ToString("dd/MM/yyyy"),
                        toDate = nextDate.ToString("dd/MM/yyyy"),
                        duration = $"{days} Days",
                        intAccrued = Math.Round(interestAccrued, 2),
                        totAccrued = Math.Round(totalAccrued, 2),
                        intPaid = 0m,
                        principalPaid = 0m,
                        addedPrincipal = 0m,
                        adjPrincipal = 0m,
                        newPrincipal = currentPrincipal,
                        isPaid = false
                    });

                    currentDate = nextDate;
                }
            }

            return statements;
        }

        // Helper method to generate interest statements
        private List<object> GenerateInterestStatements(GoldLoan loan, DateTime fromDate, DateTime toDate, decimal openingPrincipal)
        {
            var statements = new List<object>();
            var currentDate = fromDate;
            var totalAccrued = 0m;
            var currentPrincipal = openingPrincipal;

            while (currentDate < toDate)
            {
                var nextDate = currentDate.AddDays(30) > toDate ? toDate : currentDate.AddDays(30);
                var days = (nextDate - currentDate).Days;

                var interestAccrued = (currentPrincipal * loan.InterestRate * days) / (365 * 100);
                totalAccrued += interestAccrued;

                statements.Add(new
                {
                    fromDate = currentDate.ToString("dd/MM/yyyy"),
                    toDate = nextDate.ToString("dd/MM/yyyy"),
                    duration = $"{days} Days",
                    intAccrued = Math.Round(interestAccrued, 2),
                    totAccrued = Math.Round(totalAccrued, 2),
                    intPaid = 0m,
                    principalPaid = 0m,
                    addedPrincipal = 0m,
                    adjPrincipal = 0m,
                    newPrincipal = currentPrincipal
                });

                currentDate = nextDate;
            }

            return statements;
        }

        // GET: api/receipts/generate-receipt-number
        [HttpGet("generate-receipt-number")]
        public async Task<ActionResult<object>> GenerateReceiptNumber()
        {
            try
            {
                var today = DateTime.UtcNow;
                var prefix = $"RED{today:yyMMdd}";

                var lastReceipt = await _context.Receipts
                    .Where(r => r.ReceiptNumber.StartsWith(prefix))
                    .OrderByDescending(r => r.ReceiptNumber)
                    .FirstOrDefaultAsync();

                string receiptNumber;
                if (lastReceipt != null && lastReceipt.ReceiptNumber.Length > prefix.Length)
                {
                    var lastSequence = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                    if (int.TryParse(lastSequence, out int sequence))
                    {
                        receiptNumber = $"{prefix}{(sequence + 1):D4}";
                    }
                    else
                    {
                        receiptNumber = $"{prefix}0001";
                    }
                }
                else
                {
                    receiptNumber = $"{prefix}0001";
                }

                return Ok(new
                {
                    success = true,
                    message = "Receipt number generated successfully",
                    data = receiptNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating receipt number",
                    error = ex.Message
                });
            }
        }

        // POST: api/receipts
        [HttpPost]
        public async Task<ActionResult<object>> CreateReceipt([FromBody] Receipt receipt)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate loan exists
                var loan = await _context.GoldLoans
                    .Include(gl => gl.Scheme)
                    .FirstOrDefaultAsync(gl => gl.Id == receipt.GoldLoanId);

                if (loan == null)
                {
                    return BadRequest(new { success = false, message = "Invalid loan ID" });
                }

                // Get last receipt to determine starting point
                var lastReceipt = await _context.Receipts
                    .Where(r => r.GoldLoanId == receipt.GoldLoanId && r.Status == "Completed")
                    .OrderByDescending(r => r.TillDate)
                    .ThenByDescending(r => r.CreatedDate)
                    .FirstOrDefaultAsync();

                // Calculate advance interest end date
                var advanceMonths = loan.Scheme?.AdvanceMonth ?? 0;
                var advanceInterestEndDate = loan.LoanDate.AddMonths(advanceMonths);

                // Determine the start date for interest accrual
                DateTime interestAccrualStartDate;
                if (lastReceipt != null)
                {
                    interestAccrualStartDate = lastReceipt.TillDate;
                }
                else
                {
                    // First receipt - start after advance interest period
                    interestAccrualStartDate = advanceInterestEndDate;
                }

                // Apply grace days
                var graceDays = loan.Scheme?.GraceDays ?? 0;
                DateTime interestStartDate = interestAccrualStartDate.AddDays(graceDays);

                // Calculate total days due
                int totalDaysDue = (receipt.ReceiptDate - interestStartDate).Days;

                if (totalDaysDue < 0)
                {
                    totalDaysDue = 0;
                }

                // Calculate total interest due for the period
                decimal outstandingPrincipal = lastReceipt?.OutstandingPrincipal ?? loan.LoanAmount;
                decimal totalInterestDue = 0;

                if (totalDaysDue > 0)
                {
                    var calculationMethod = loan.Scheme?.CalculationMethod ?? "Simple";
                    totalInterestDue = CalculateInterestAmount(
                        outstandingPrincipal,
                        loan.InterestRate,
                        totalDaysDue,
                        calculationMethod
                    );
                }

                // Calculate TillDate based on actual interest paid
                DateTime calculatedTillDate;

                if (totalDaysDue == 0 || totalInterestDue == 0)
                {
                    // No interest due, TillDate is same as receipt date
                    calculatedTillDate = receipt.ReceiptDate;
                }
                else if (receipt.InterestAmount >= totalInterestDue)
                {
                    // Paying full interest due, TillDate is receipt date
                    calculatedTillDate = receipt.ReceiptDate;
                }
                else
                {
                    // Partial payment - calculate how many days they're paying for
                    decimal dailyInterest = totalInterestDue / totalDaysDue;
                    int daysPaidFor = (int)(receipt.InterestAmount / dailyInterest);

                    // TillDate = last paid date + days covered by this payment
                    calculatedTillDate = interestStartDate.AddDays(daysPaidFor);

                    // Ensure TillDate doesn't exceed receipt date
                    if (calculatedTillDate > receipt.ReceiptDate)
                    {
                        calculatedTillDate = receipt.ReceiptDate;
                    }
                }

                // Set the calculated TillDate
                receipt.TillDate = calculatedTillDate;

                // Generate receipt number if not provided
                if (string.IsNullOrEmpty(receipt.ReceiptNumber))
                {
                    var receiptNumberResponse = await GenerateReceiptNumber();
                    if (receiptNumberResponse.Value is OkObjectResult okResult && okResult.Value != null)
                    {
                        var result = okResult.Value as dynamic;
                        receipt.ReceiptNumber = result?.data ?? string.Empty;
                    }
                }

                receipt.Id = Guid.NewGuid();
                receipt.CreatedDate = DateTime.UtcNow;
                receipt.Status = "Completed";

                // Update outstanding principal
                receipt.OutstandingPrincipal =
                                outstandingPrincipal - (receipt?.PrincipalAmount ?? 0m);

                // Set interest statements
                if (receipt.InterestStatements != null)
                {
                    foreach (var statement in receipt.InterestStatements)
                    {
                        statement.Id = Guid.NewGuid();
                        statement.ReceiptId = receipt.Id;
                        statement.GoldLoanId = receipt.GoldLoanId;
                        statement.CreatedDate = DateTime.UtcNow;
                    }
                }

                // Set payment modes
                if (receipt.PaymentModes != null)
                {
                    foreach (var paymentMode in receipt.PaymentModes)
                    {
                        paymentMode.Id = Guid.NewGuid();
                        paymentMode.ReceiptId = receipt.Id;
                        paymentMode.CreatedDate = DateTime.UtcNow;
                    }
                }

                // Update loan status if full payment
                if (receipt.PaymentType == "full" && receipt.OutstandingPrincipal == 0)
                {
                    loan.Status = "Closed";
                    loan.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    loan.Status = "Active";
                    loan.UpdatedDate = DateTime.UtcNow;
                }

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, new
                {
                    success = true,
                    message = "Receipt created successfully",
                    data = new
                    {
                        receipt.Id,
                        receipt.ReceiptNumber,
                        receiptDate = receipt.ReceiptDate,
                        tillDate = receipt.TillDate,
                        interestPaid = receipt.InterestAmount,
                        totalInterestDue = Math.Round(totalInterestDue, 2),
                        daysCovered = (calculatedTillDate - interestStartDate).Days,
                        totalDaysDue = totalDaysDue,
                        note = receipt.InterestAmount < totalInterestDue
                            ? "Partial interest payment - unpaid interest will accumulate"
                            : null
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error creating receipt",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/receipts/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteReceipt(Guid id)
        {
            try
            {
                var receipt = await _context.Receipts
                    .Include(r => r.InterestStatements)
                    .Include(r => r.PaymentModes)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (receipt == null)
                {
                    return NotFound(new { success = false, message = "Receipt not found" });
                }

                // Mark as cancelled instead of deleting
                receipt.Status = "Cancelled";
                receipt.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Receipt cancelled successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting receipt",
                    error = ex.Message
                });
            }
        }
    }
}