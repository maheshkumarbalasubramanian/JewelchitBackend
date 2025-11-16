using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Models;

namespace JewelChitApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CompaniesController> _logger;
        private const string LogoUploadFolder = "uploads/company-logos";

        public CompaniesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<CompaniesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetCompanies(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? type,
            [FromQuery] int? minCustomers,
            [FromQuery] int? maxCustomers)
        {
            var query = _context.Companies.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.CompanyName.ToLower().Contains(search.ToLower()) ||
                    c.CompanyCode.ToLower().Contains(search.ToLower()) ||
                    (c.Description != null && c.Description.ToLower().Contains(search.ToLower())));
            }

            if (status == "active")
                query = query.Where(c => c.IsActive);
            else if (status == "inactive")
                query = query.Where(c => !c.IsActive);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(c => c.CompanyType == type);

            if (minCustomers.HasValue)
                query = query.Where(c => c.CustomerCount >= minCustomers.Value);

            if (maxCustomers.HasValue)
                query = query.Where(c => c.CustomerCount <= maxCustomers.Value);

            var companies = await query
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyResponse
                {
                    Id = c.Id,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    CompanyType = c.CompanyType,
                    Description = c.Description,
                    LogoUrl = c.LogoPath != null ? $"/api/companies/{c.Id}/logo" : null,
                    Pincode = c.Pincode,
                    IsActive = c.IsActive,
                    CustomerCount = c.CustomerCount,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .ToListAsync();

            return Ok(companies);
        }


        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<CompanyDropdownResponse>>> GetCompaniesDropdown()
        {
            var companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .Select(c => new CompanyDropdownResponse
                {
                    Id = c.Id,
                    Code = c.CompanyCode,
                    Name = c.CompanyName
                })
                .ToListAsync();

            return Ok(companies);
        }

        // GET: api/companies/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyResponse>> GetCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return NotFound(new { message = "Company not found" });

            var response = new CompanyResponse
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                CompanyType = company.CompanyType,
                Description = company.Description,
                LogoUrl = company.LogoPath != null ? $"/api/companies/{company.Id}/logo" : null,
                Pincode = company.Pincode,
                IsActive = company.IsActive,
                CustomerCount = company.CustomerCount,
                CreatedDate = company.CreatedDate,
                UpdatedDate = company.UpdatedDate
            };

            return Ok(response);
        }

        // GET: api/companies/stats
        [HttpGet("stats")]
        public async Task<ActionResult<CompanyStatsResponse>> GetCompanyStats()
        {
            var total = await _context.Companies.CountAsync();
            var active = await _context.Companies.CountAsync(c => c.IsActive);
            var inactive = total - active;
            var recent = await _context.Companies
                .CountAsync(c => c.CreatedDate >= DateTime.UtcNow.AddDays(-7));

            return Ok(new CompanyStatsResponse
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Recent = recent
            });
        }

        // POST: api/companies
        [HttpPost]
        public async Task<ActionResult<CompanyResponse>> CreateCompany([FromForm] AddCompanyRequest request)
        {
            // Check for duplicate company code
            if (await _context.Companies.AnyAsync(c => c.CompanyCode == request.CompanyCode))
            {
                return Conflict(new { message = "Company code already exists" });
            }

            var company = new Company
            {
                Id = Guid.NewGuid(),
                CompanyCode = request.CompanyCode.ToUpper(),
                CompanyName = request.CompanyName,
                CompanyType = request.CompanyType,
                Description = request.Description,
                Pincode = request.Pincode,
                IsActive = true,
                CustomerCount = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name
            };

            // Handle logo upload
            if (request.Logo != null && request.Logo.Length > 0)
            {
                try
                {
                    company.LogoPath = await SaveLogoAsync(request.Logo, company.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading logo for company {CompanyCode}", request.CompanyCode);
                    return BadRequest(new { message = "Error uploading logo", error = ex.Message });
                }
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var response = new CompanyResponse
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                CompanyType = company.CompanyType,
                Description = company.Description,
                LogoUrl = company.LogoPath != null ? $"/api/companies/{company.Id}/logo" : null,
                Pincode = company.Pincode,
                IsActive = company.IsActive,
                CustomerCount = company.CustomerCount,
                CreatedDate = company.CreatedDate,
                UpdatedDate = company.UpdatedDate
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, response);
        }

        // PUT: api/companies/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromForm] UpdateCompanyRequest request)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return NotFound(new { message = "Company not found" });

            company.CompanyName = request.CompanyName;
            company.CompanyType = request.CompanyType;
            company.Description = request.Description;
            company.Pincode = request.Pincode;
            company.IsActive = request.IsActive;
            company.UpdatedDate = DateTime.UtcNow;
            company.UpdatedBy = User?.Identity?.Name;

            // Handle logo update
            if (request.RemoveLogo && !string.IsNullOrEmpty(company.LogoPath))
            {
                DeleteLogoFile(company.LogoPath);
                company.LogoPath = null;
            }
            else if (request.Logo != null && request.Logo.Length > 0)
            {
                // Delete old logo if exists
                if (!string.IsNullOrEmpty(company.LogoPath))
                {
                    DeleteLogoFile(company.LogoPath);
                }

                try
                {
                    company.LogoPath = await SaveLogoAsync(request.Logo, company.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading logo for company {CompanyId}", id);
                    return BadRequest(new { message = "Error uploading logo", error = ex.Message });
                }
            }

            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/companies/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return NotFound(new { message = "Company not found" });

            // Check if company has customers
            if (company.CustomerCount > 0)
            {
                return BadRequest(new { message = "Cannot delete company with existing customers" });
            }

            // Delete logo file if exists
            if (!string.IsNullOrEmpty(company.LogoPath))
            {
                DeleteLogoFile(company.LogoPath);
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/companies/{id}/logo
        [HttpGet("{id}/logo")]
        public async Task<IActionResult> GetCompanyLogo(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null || string.IsNullOrEmpty(company.LogoPath))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, company.LogoPath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType);
        }

        // POST: api/companies/bulk-activate
        [HttpPost("bulk-activate")]
        public async Task<IActionResult> BulkActivate([FromBody] List<Guid> ids)
        {
            var companies = await _context.Companies
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            foreach (var company in companies)
            {
                company.IsActive = true;
                company.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{companies.Count} companies activated" });
        }

        // POST: api/companies/bulk-deactivate
        [HttpPost("bulk-deactivate")]
        public async Task<IActionResult> BulkDeactivate([FromBody] List<Guid> ids)
        {
            var companies = await _context.Companies
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            foreach (var company in companies)
            {
                company.IsActive = false;
                company.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{companies.Count} companies deactivated" });
        }

        // Helper methods
        private async Task<string> SaveLogoAsync(IFormFile logo, Guid companyId)
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, LogoUploadFolder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileExtension = Path.GetExtension(logo.FileName);
            var fileName = $"{companyId}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logo.CopyToAsync(stream);
            }

            return $"{LogoUploadFolder}/{fileName}";
        }

        private void DeleteLogoFile(string logoPath)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, logoPath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logo file: {LogoPath}", logoPath);
            }
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
