using JewelChitApplication.Data;
using JewelChitApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelChitApplication.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyResponse>> GetAllCompaniesAsync(CompanyFilter? filter = null);
        Task<CompanyResponse?> GetCompanyByIdAsync(Guid id);
        Task<CompanyStatsResponse> GetCompanyStatsAsync();
        Task<CompanyResponse> CreateCompanyAsync(AddCompanyRequest request, string? userId = null);
        Task<bool> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, string? userId = null);
        Task<bool> DeleteCompanyAsync(Guid id);
        Task<int> BulkActivateAsync(List<Guid> ids);
        Task<int> BulkDeactivateAsync(List<Guid> ids);
        Task<bool> CompanyCodeExistsAsync(string companyCode);
    }

    // Implementation
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CompanyService> _logger;
        private const string LogoUploadFolder = "uploads/company-logos";

        public CompanyService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<CompanyService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IEnumerable<CompanyResponse>> GetAllCompaniesAsync(CompanyFilter? filter = null)
        {
            var query = _context.Companies.AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(c =>
                        c.CompanyName.ToLower().Contains(search) ||
                        c.CompanyCode.ToLower().Contains(search) ||
                        (c.Description != null && c.Description.ToLower().Contains(search)));
                }

                if (filter.Status == "active")
                    query = query.Where(c => c.IsActive);
                else if (filter.Status == "inactive")
                    query = query.Where(c => !c.IsActive);

                if (!string.IsNullOrWhiteSpace(filter.Type))
                    query = query.Where(c => c.CompanyType == filter.Type);

                if (filter.CustomerRange != null)
                {
                    query = query.Where(c =>
                        c.CustomerCount >= filter.CustomerRange.Min &&
                        c.CustomerCount <= filter.CustomerRange.Max);
                }
            }

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

            return companies;
        }

        public async Task<CompanyResponse?> GetCompanyByIdAsync(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return null;

            return new CompanyResponse
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
        }

        public async Task<CompanyStatsResponse> GetCompanyStatsAsync()
        {
            var total = await _context.Companies.CountAsync();
            var active = await _context.Companies.CountAsync(c => c.IsActive);
            var inactive = total - active;
            var recent = await _context.Companies
                .CountAsync(c => c.CreatedDate >= DateTime.UtcNow.AddDays(-7));

            return new CompanyStatsResponse
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Recent = recent
            };
        }

        public async Task<CompanyResponse> CreateCompanyAsync(AddCompanyRequest request, string? userId = null)
        {
            if (await CompanyCodeExistsAsync(request.CompanyCode))
            {
                throw new InvalidOperationException("Company code already exists");
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
                CreatedBy = userId
            };

            if (request.Logo != null && request.Logo.Length > 0)
            {
                company.LogoPath = await SaveLogoAsync(request.Logo, company.Id);
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return await GetCompanyByIdAsync(company.Id) ?? throw new Exception("Failed to create company");
        }

        public async Task<bool> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, string? userId = null)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return false;

            company.CompanyName = request.CompanyName;
            company.CompanyType = request.CompanyType;
            company.Description = request.Description;
            company.Pincode = request.Pincode;
            company.IsActive = request.IsActive;
            company.UpdatedDate = DateTime.UtcNow;
            company.UpdatedBy = userId;

            if (request.RemoveLogo && !string.IsNullOrEmpty(company.LogoPath))
            {
                DeleteLogoFile(company.LogoPath);
                company.LogoPath = null;
            }
            else if (request.Logo != null && request.Logo.Length > 0)
            {
                if (!string.IsNullOrEmpty(company.LogoPath))
                {
                    DeleteLogoFile(company.LogoPath);
                }

                company.LogoPath = await SaveLogoAsync(request.Logo, company.Id);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCompanyAsync(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return false;

            if (company.CustomerCount > 0)
            {
                throw new InvalidOperationException("Cannot delete company with existing customers");
            }

            if (!string.IsNullOrEmpty(company.LogoPath))
            {
                DeleteLogoFile(company.LogoPath);
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> BulkActivateAsync(List<Guid> ids)
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
            return companies.Count;
        }

        public async Task<int> BulkDeactivateAsync(List<Guid> ids)
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
            return companies.Count;
        }

        public async Task<bool> CompanyCodeExistsAsync(string companyCode)
        {
            return await _context.Companies
                .AnyAsync(c => c.CompanyCode == companyCode.ToUpper());
        }

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
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting logo file: {LogoPath}", logoPath);
            }
        }
    }
}
