
public interface ICustomerService
{
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto customerDto);
    Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();
    Task<CustomerResponseDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto customerDto);
    Task DeleteCustomerAsync(Guid customerId); // Soft delete
    Task<CustomerResponseDto?> GetCustomerByNationalIdAsync(string nationalId);
}