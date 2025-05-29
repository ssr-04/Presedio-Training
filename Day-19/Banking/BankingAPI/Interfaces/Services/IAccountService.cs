public interface IAccountService
{
    Task<AccountResponseDto> CreateAccountAsync(CreateAccountDto accountDto);
    Task<AccountResponseDto?> GetAccountByIdAsync(Guid accountId);
    Task<IEnumerable<AccountResponseDto>> GetAccountsByCustomerIdAsync(Guid customerId);
    Task<AccountResponseDto?> GetAccountByAccountNumberAsync(string accountNumber);
    Task DeleteAccountAsync(Guid accountId); // Soft delete
}
