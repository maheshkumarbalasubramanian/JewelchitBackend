using Microsoft.AspNetCore.Mvc;
using JewelChitApplication.DTOs;
using JewelChitApplication.Services;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JewelChitApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Get all customers with filtering, sorting, and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<CustomerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponseDto<CustomerResponseDto>>> GetCustomers([FromQuery] CustomerSearchFilterDto filter)
        {
            try
            {
                var result = await _customerService.GetCustomersAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return BadRequest(new { message = "Error retrieving customers", error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerResponseDto>> GetCustomerById(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found" });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
                return BadRequest(new { message = "Error retrieving customer", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerResponseDto>> CreateCustomer([FromBody] CustomerRequestDto customerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdBy = User?.Identity?.Name ?? "System";
                var customer = await _customerService.CreateCustomerAsync(customerDto, createdBy);

                return CreatedAtAction(
                    nameof(GetCustomerById),
                    new { id = customer.Id },
                    customer
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating customer");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return BadRequest(new { message = "Error creating customer", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerResponseDto>> UpdateCustomer(Guid id, [FromBody] CustomerRequestDto customerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedBy = User?.Identity?.Name ?? "System";
                var customer = await _customerService.UpdateCustomerAsync(id, customerDto, updatedBy);

                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer not found {CustomerId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating customer {CustomerId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return BadRequest(new { message = "Error updating customer", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return BadRequest(new { message = "Error deleting customer", error = ex.Message });
            }
        }

        /// <summary>
        /// Update customer status
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomerStatus(Guid id, [FromBody] UpdateCustomerStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedBy = User?.Identity?.Name ?? "System";
                var result = await _customerService.UpdateCustomerStatusAsync(id, statusDto.Status, updatedBy);

                if (!result)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found" });
                }

                return Ok(new { message = "Customer status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer status {CustomerId}", id);
                return BadRequest(new { message = "Error updating customer status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(CustomerStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CustomerStatsDto>> GetCustomerStatistics()
        {
            try
            {
                var stats = await _customerService.GetCustomerStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics");
                return BadRequest(new { message = "Error retrieving statistics", error = ex.Message });
            }
        }



        /// </summary>
        [HttpGet("\"{customerId}/image\"")]
        [ProducesResponseType(typeof(CustomerStatsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerImage(Guid customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                    
                if (customer == null)
                {
                    return NotFound(new { success = false, message = "Customer not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Customer image retrieved successfully",
                    data = customer.ProfileImage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer Profile image");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Generate a new unique customer code
        /// </summary>
        [HttpGet("generate-code")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GenerateCustomerCode()
        {
            try
            {
                var customerCode = await _customerService.GenerateCustomerCodeAsync();
                return Ok(new { customerCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer code");
                return BadRequest(new { message = "Error generating customer code", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if customer code is unique
        /// </summary>
        [HttpGet("check-code/{customerCode}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CheckCustomerCode(string customerCode)
        {
            try
            {
                var isUnique = await _customerService.IsCustomerCodeUniqueAsync(customerCode);
                return Ok(new { customerCode, isUnique });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer code");
                return BadRequest(new { message = "Error checking customer code", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if mobile number is unique
        /// </summary>
        [HttpGet("check-mobile/{mobile}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CheckMobileNumber(string mobile, [FromQuery] Guid? excludeCustomerId = null)
        {
            try
            {
                var isUnique = await _customerService.IsMobileUniqueAsync(mobile, excludeCustomerId);
                return Ok(new { mobile, isUnique });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking mobile number");
                return BadRequest(new { message = "Error checking mobile number", error = ex.Message });
            }
        }
    }
}
