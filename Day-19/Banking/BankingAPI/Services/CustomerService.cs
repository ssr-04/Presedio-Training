using AutoMapper;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto customerDto)
    {
        if (await _customerRepository.GetCustomerByNationalIdAsync(customerDto.NationalId) != null)
        {
            throw new InvalidOperationException($"Customer with National ID '{customerDto.NationalId}' already exists.");
        }
        if ((await _customerRepository.FindAsync(c => c.Email == customerDto.Email && c.IsActive)).Any())
        {
                throw new InvalidOperationException($"Customer with Email '{customerDto.Email}' already exists.");
        }

        var customer = _mapper.Map<Customer>(customerDto);
        customer.CustomerId = Guid.NewGuid();
        customer.RegistrationDate = DateTimeOffset.UtcNow;
        customer.IsActive = true;

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return _mapper.Map<CustomerResponseDto>(customer);
    }

    public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId); 
        return customer != null && customer.IsActive ? _mapper.Map<CustomerResponseDto>(customer) : null;
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto customerDto)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null || !customer.IsActive)
        {
            throw new KeyNotFoundException($"Customer with ID '{customerId}' not found or is inactive.");
        }

        _mapper.Map(customerDto, customer);

        if (customerDto.Email != null && (await _customerRepository.FindAsync(c => c.Email == customerDto.Email && c.CustomerId != customerId && c.IsActive)).Any())
        {
                throw new InvalidOperationException($"Email '{customerDto.Email}' is already registered to another active customer.");
        }

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return _mapper.Map<CustomerResponseDto>(customer);
    }

    public async Task DeleteCustomerAsync(Guid customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null || !customer.IsActive)
        {
            throw new KeyNotFoundException($"Customer with ID '{customerId}' not found or already inactive.");
        }

        await _customerRepository.DeleteAsync(customerId);
        await _customerRepository.SaveChangesAsync();
    }

    public async Task<CustomerResponseDto?> GetCustomerByNationalIdAsync(string nationalId)
    {
        var customer = await _customerRepository.GetCustomerByNationalIdAsync(nationalId); 
        return customer != null && customer.IsActive ? _mapper.Map<CustomerResponseDto>(customer) : null;
    }
}