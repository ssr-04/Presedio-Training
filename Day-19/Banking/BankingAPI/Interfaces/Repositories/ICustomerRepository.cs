public interface ICustomerRepository : IGenericRepository<Customer, Guid>
{
    Task<Customer?> GetCustomerByNationalIdAsync(string nationalId);
}