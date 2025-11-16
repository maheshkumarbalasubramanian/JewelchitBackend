using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JewelChitApplication.DTOs;
using JewelChitApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using JewelChitApplication.Data;

namespace JewelChitApplication.Services
{
    public interface ICustomerService
    {
        Task<PaginatedResponseDto<CustomerResponseDto>> GetCustomersAsync(CustomerSearchFilterDto filter);
        Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid id);
        Task<CustomerResponseDto?> GetCustomerImageByIdAsync(Guid id);
        Task<CustomerResponseDto> CreateCustomerAsync(CustomerRequestDto customerDto, string? createdBy = null);
        Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, CustomerRequestDto customerDto, string? updatedBy = null);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<bool> UpdateCustomerStatusAsync(Guid id, string status, string? updatedBy = null);
        Task<CustomerStatsDto> GetCustomerStatsAsync();
        Task<string> GenerateCustomerCodeAsync();
        Task<bool> IsCustomerCodeUniqueAsync(string customerCode);
        Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeCustomerId = null);
    }
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponseDto<CustomerResponseDto>> GetCustomersAsync(CustomerSearchFilterDto filter)
        {
            var query = _context.Customers
                .Include(c => c.AddressInfo)
                .Include(c => c.ContactInfo)
                .Include(c => c.OtherInfo)
                .Include(c => c.Verification)
                .Include(c => c.Documents)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.CustomerName.ToLower().Contains(searchTerm) ||
                    c.CustomerCode.ToLower().Contains(searchTerm) ||
                    c.Mobile.Contains(searchTerm) ||
                    (c.ContactInfo != null && c.ContactInfo.Email != null && c.ContactInfo.Email.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Area))
            {
                query = query.Where(c => c.AddressInfo != null && c.AddressInfo.Area == filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(c => c.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.CustomerCode))
            {
                query = query.Where(c => c.CustomerCode.ToLower().Contains(filter.CustomerCode.ToLower()));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "customername" => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.CustomerName)
                    : query.OrderBy(c => c.CustomerName),
                "customercode" => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.CustomerCode)
                    : query.OrderBy(c => c.CustomerCode),
                "mobile" => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Mobile)
                    : query.OrderBy(c => c.Mobile),
                "createdat" => filter.SortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),
                _ => query.OrderBy(c => c.CustomerName)
            };

            // Apply pagination
            var customers = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var customerDtos = customers.Select(MapToResponseDto).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return new PaginatedResponseDto<CustomerResponseDto>
            {
                Data = customerDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filter.PageNumber > 1,
                HasNextPage = filter.PageNumber < totalPages
            };
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _context.Customers
                .Include(c => c.AddressInfo)
                .Include(c => c.ContactInfo)
                .Include(c => c.OtherInfo)
                .Include(c => c.Verification)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            return customer == null ? null : MapToResponseDto(customer);
        }

        public async Task<CustomerResponseDto> CreateCustomerAsync(CustomerRequestDto customerDto, string? createdBy = null)
        {
            // Validate unique constraints
            if (!await IsCustomerCodeUniqueAsync(customerDto.CustomerCode))
            {
                throw new InvalidOperationException($"Customer code '{customerDto.CustomerCode}' already exists.");
            }

            if (!await IsMobileUniqueAsync(customerDto.Mobile))
            {
                throw new InvalidOperationException($"Mobile number '{customerDto.Mobile}' is already registered.");
            }

            var customer = new Customer
            {
                CustomerCode = customerDto.CustomerCode,
                CustomerName = customerDto.CustomerName,
                RelationshipName = customerDto.RelationshipName,
                RelationName = customerDto.RelationName,
                Mobile = customerDto.Mobile,
                ProfileImage = customerDto.ProfileImage,
                DateOfBirth = customerDto.DateOfBirth?.ToUniversalTime(),
                Status = "ACTIVE",
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            _context.Customers.Add(customer);

            // Add Address Info
            var addressInfo = new CustomerAddressInfo
            {
                CustomerId = customer.Id,
                Address1 = customerDto.AddressInfo.Address1,
                Address2 = customerDto.AddressInfo.Address2,
                Address3 = customerDto.AddressInfo.Address3,
                Address4 = customerDto.AddressInfo.Address4,
                Area = customerDto.AddressInfo.Area,
                State = customerDto.AddressInfo.State,
                Pincode = customerDto.AddressInfo.Pincode,
                City = customerDto.AddressInfo.City
            };
            _context.CustomerAddressInfo.Add(addressInfo);

            // Add Contact Info
            var contactInfo = new CustomerContactInfo
            {
                CustomerId = customer.Id,
                PrimaryPhone = customerDto.ContactInfo.PrimaryPhone,
                SecondaryPhone = customerDto.ContactInfo.SecondaryPhone,
                Email = customerDto.ContactInfo.Email,
                Whatsapp = customerDto.ContactInfo.Whatsapp
            };
            _context.CustomerContactInfo.Add(contactInfo);

            // Add Other Info
            var otherInfo = new CustomerOtherInfo
            {
                CustomerId = customer.Id,
                AadharNumber = customerDto.OtherInfo.AadharNumber,
                PanNumber = customerDto.OtherInfo.PanNumber,
                Occupation = customerDto.OtherInfo.Occupation,
                MonthlyIncome = customerDto.OtherInfo.MonthlyIncome,
                ReferenceBy = customerDto.OtherInfo.ReferenceBy
            };
            _context.CustomerOtherInfo.Add(otherInfo);

            // Add Verification
            var verification = new CustomerVerification
            {
                CustomerId = customer.Id,
                AadharVerified = customerDto.Verification.AadharVerified,
                FingerprintVerified = customerDto.Verification.FingerprintVerified,
                AadharVerifiedAt = customerDto.Verification.AadharVerified ? DateTime.UtcNow : null,
                FingerprintVerifiedAt = customerDto.Verification.FingerprintVerified ? DateTime.UtcNow : null
            };
            _context.CustomerVerification.Add(verification);

            // Add Documents
            if (!string.IsNullOrWhiteSpace(customerDto.Documents.AadharDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "AadharDocument",
                    DocumentData = customerDto.Documents.AadharDocument
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.PanDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "PanDocument",
                    DocumentData = customerDto.Documents.PanDocument
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.IncomeProof))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "IncomeProof",
                    DocumentData = customerDto.Documents.IncomeProof
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.AddressProof))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "AddressProof",
                    DocumentData = customerDto.Documents.AddressProof
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.OtherDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "OtherDocument",
                    DocumentData = customerDto.Documents.OtherDocument
                });
            }

            await _context.SaveChangesAsync();

            // Reload customer with all related data
            customer = await _context.Customers
                .Include(c => c.AddressInfo)
                .Include(c => c.ContactInfo)
                .Include(c => c.OtherInfo)
                .Include(c => c.Verification)
                .Include(c => c.Documents)
                .FirstAsync(c => c.Id == customer.Id);

            return MapToResponseDto(customer);
        }

        public async Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, CustomerRequestDto customerDto, string? updatedBy = null)
        {
            var customer = await _context.Customers
                .Include(c => c.AddressInfo)
                .Include(c => c.ContactInfo)
                .Include(c => c.OtherInfo)
                .Include(c => c.Verification)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }

            // Validate unique constraints (excluding current customer)
            if (!await IsMobileUniqueAsync(customerDto.Mobile, id))
            {
                throw new InvalidOperationException($"Mobile number '{customerDto.Mobile}' is already registered.");
            }

            // Update customer basic info
            customer.CustomerName = customerDto.CustomerName;
            customer.RelationshipName = customerDto.RelationshipName;
            customer.RelationName = customerDto.RelationName;
            customer.Mobile = customerDto.Mobile;
            customer.ProfileImage = customerDto.ProfileImage;
            customer.DateOfBirth = customerDto.DateOfBirth?.ToUniversalTime();
            customer.UpdatedBy = updatedBy;
            customer.UpdatedAt = DateTime.UtcNow;

            // Update Address Info
            if (customer.AddressInfo != null)
            {
                customer.AddressInfo.Address1 = customerDto.AddressInfo.Address1;
                customer.AddressInfo.Address2 = customerDto.AddressInfo.Address2;
                customer.AddressInfo.Address3 = customerDto.AddressInfo.Address3;
                customer.AddressInfo.Address4 = customerDto.AddressInfo.Address4;
                customer.AddressInfo.Area = customerDto.AddressInfo.Area;
                customer.AddressInfo.State = customerDto.AddressInfo.State;
                customer.AddressInfo.Pincode = customerDto.AddressInfo.Pincode;
                customer.AddressInfo.City = customerDto.AddressInfo.City;
            }

            // Update Contact Info
            if (customer.ContactInfo != null)
            {
                customer.ContactInfo.PrimaryPhone = customerDto.ContactInfo.PrimaryPhone;
                customer.ContactInfo.SecondaryPhone = customerDto.ContactInfo.SecondaryPhone;
                customer.ContactInfo.Email = customerDto.ContactInfo.Email;
                customer.ContactInfo.Whatsapp = customerDto.ContactInfo.Whatsapp;
            }

            // Update Other Info
            if (customer.OtherInfo != null)
            {
                customer.OtherInfo.AadharNumber = customerDto.OtherInfo.AadharNumber;
                customer.OtherInfo.PanNumber = customerDto.OtherInfo.PanNumber;
                customer.OtherInfo.Occupation = customerDto.OtherInfo.Occupation;
                customer.OtherInfo.MonthlyIncome = customerDto.OtherInfo.MonthlyIncome;
                customer.OtherInfo.ReferenceBy = customerDto.OtherInfo.ReferenceBy;
            }

            // Update Verification
            if (customer.Verification != null)
            {
                customer.Verification.AadharVerified = customerDto.Verification.AadharVerified;
                customer.Verification.FingerprintVerified = customerDto.Verification.FingerprintVerified;

                if (customerDto.Verification.AadharVerified && customer.Verification.AadharVerifiedAt == null)
                {
                    customer.Verification.AadharVerifiedAt = DateTime.UtcNow;
                }

                if (customerDto.Verification.FingerprintVerified && customer.Verification.FingerprintVerifiedAt == null)
                {
                    customer.Verification.FingerprintVerifiedAt = DateTime.UtcNow;
                }
            }

            // Update Documents - Remove existing and add new
            _context.CustomerDocuments.RemoveRange(customer.Documents);

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.AadharDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "AadharDocument",
                    DocumentData = customerDto.Documents.AadharDocument
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.PanDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "PanDocument",
                    DocumentData = customerDto.Documents.PanDocument
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.IncomeProof))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "IncomeProof",
                    DocumentData = customerDto.Documents.IncomeProof
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.AddressProof))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "AddressProof",
                    DocumentData = customerDto.Documents.AddressProof
                });
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Documents.OtherDocument))
            {
                _context.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = customer.Id,
                    DocumentType = "OtherDocument",
                    DocumentData = customerDto.Documents.OtherDocument
                });
            }

            await _context.SaveChangesAsync();

            return MapToResponseDto(customer);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return false;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCustomerStatusAsync(Guid id, string status, string? updatedBy = null)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return false;
            }

            customer.Status = status;
            customer.UpdatedBy = updatedBy;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CustomerStatsDto> GetCustomerStatsAsync()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var activeCustomers = await _context.Customers.CountAsync(c => c.Status == "ACTIVE");
            var verifiedCustomers = await _context.Customers
                .Include(c => c.Verification)
                .CountAsync(c => c.Verification != null &&
                    (c.Verification.AadharVerified || c.Verification.FingerprintVerified));

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentCustomers = await _context.Customers
                .CountAsync(c => c.CreatedAt >= thirtyDaysAgo);

            return new CustomerStatsDto
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                VerifiedCustomers = verifiedCustomers,
                RecentCustomers = recentCustomers
            };
        }

        public async Task<string> GenerateCustomerCodeAsync()
        {
            string customerCode;
            bool isUnique;

            do
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                var lastSixDigits = timestamp.Substring(timestamp.Length - 6);
                customerCode = $"PR{lastSixDigits}";

                isUnique = await IsCustomerCodeUniqueAsync(customerCode);
            } while (!isUnique);

            return customerCode;
        }

        public async Task<bool> IsCustomerCodeUniqueAsync(string customerCode)
        {
            return !await _context.Customers.AnyAsync(c => c.CustomerCode == customerCode);
        }

        public async Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeCustomerId = null)
        {
            var query = _context.Customers.Where(c => c.Mobile == mobile);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return !await query.AnyAsync();
        }

        private CustomerResponseDto MapToResponseDto(Customer customer)
        {
            var documents = customer.Documents.ToList();

            return new CustomerResponseDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                RelationshipName = customer.RelationshipName,
                RelationName = customer.RelationName,
                Mobile = customer.Mobile,
                ProfileImage = customer.ProfileImage,
                DateOfBirth = customer.DateOfBirth,
                Status = customer.Status,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                AddressInfo = customer.AddressInfo != null ? new AddressInfoDto
                {
                    Address1 = customer.AddressInfo.Address1,
                    Address2 = customer.AddressInfo.Address2,
                    Address3 = customer.AddressInfo.Address3,
                    Address4 = customer.AddressInfo.Address4,
                    Area = customer.AddressInfo.Area,
                    State = customer.AddressInfo.State,
                    Pincode = customer.AddressInfo.Pincode,
                    City = customer.AddressInfo.City
                } : new AddressInfoDto(),
                ContactInfo = customer.ContactInfo != null ? new ContactInfoDto
                {
                    PrimaryPhone = customer.ContactInfo.PrimaryPhone,
                    SecondaryPhone = customer.ContactInfo.SecondaryPhone,
                    Email = customer.ContactInfo.Email,
                    Whatsapp = customer.ContactInfo.Whatsapp
                } : new ContactInfoDto(),
                OtherInfo = customer.OtherInfo != null ? new OtherInfoDto
                {
                    AadharNumber = customer.OtherInfo.AadharNumber,
                    PanNumber = customer.OtherInfo.PanNumber,
                    Occupation = customer.OtherInfo.Occupation,
                    MonthlyIncome = customer.OtherInfo.MonthlyIncome,
                    ReferenceBy = customer.OtherInfo.ReferenceBy
                } : new OtherInfoDto(),
                Documents = new DocumentsDto
                {
                    AadharDocument = documents.FirstOrDefault(d => d.DocumentType == "AadharDocument")?.DocumentData,
                    PanDocument = documents.FirstOrDefault(d => d.DocumentType == "PanDocument")?.DocumentData,
                    IncomeProof = documents.FirstOrDefault(d => d.DocumentType == "IncomeProof")?.DocumentData,
                    AddressProof = documents.FirstOrDefault(d => d.DocumentType == "AddressProof")?.DocumentData,
                    OtherDocument = documents.FirstOrDefault(d => d.DocumentType == "OtherDocument")?.DocumentData
                },
                Verification = customer.Verification != null ? new VerificationDto
                {
                    AadharVerified = customer.Verification.AadharVerified,
                    FingerprintVerified = customer.Verification.FingerprintVerified
                } : new VerificationDto()
            };
        }

        public async Task<CustomerResponseDto?> GetCustomerImageByIdAsync(Guid id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            return customer == null ? null : MapToResponseDto(customer);
        }
    }
}
