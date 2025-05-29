using AutoMapper;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository; // Needed to validate customer existence
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository accountRepository, ICustomerRepository customerRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<AccountResponseDto> CreateAccountAsync(CreateAccountDto accountDto)
    {
        var customer = await _customerRepository.GetByIdAsync(accountDto.CustomerId); // Fetch customer to ensure existence
        if (customer == null || !customer.IsActive)
        {
            throw new KeyNotFoundException($"Customer with ID '{accountDto.CustomerId}' not found or is inactive.");
        }

        string newAccountNumber;
        do
        {
            newAccountNumber = GenerateAccountNumber();
        } while (await _accountRepository.GetAccountByAccountNumberAsync(newAccountNumber) != null);

        var account = _mapper.Map<Account>(accountDto);
        account.AccountId = Guid.NewGuid();
        account.AccountNumber = newAccountNumber;
        account.OpeningDate = DateTimeOffset.UtcNow;
        account.Status = AccountStatus.Active;
        account.IsActive = true;
        account.Balance = accountDto.InitialDeposit;

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

        account.Customer = customer; // Manually assign customer to account for correct DTO mapping

        return _mapper.Map<AccountResponseDto>(account);
    }

    public async Task<AccountResponseDto?> GetAccountByIdAsync(Guid accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId); 
        return account != null && account.IsActive ? _mapper.Map<AccountResponseDto>(account) : null;
    }

    public async Task<IEnumerable<AccountResponseDto>> GetAccountsByCustomerIdAsync(Guid customerId)
    {
        var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);
        var activeAccounts = accounts.Where(a => a.IsActive);
        return _mapper.Map<IEnumerable<AccountResponseDto>>(activeAccounts);
    }

    public async Task<AccountResponseDto?> GetAccountByAccountNumberAsync(string accountNumber)
    {
        var account = await _accountRepository.GetAccountByAccountNumberAsync(accountNumber); 
        return account != null && account.IsActive ? _mapper.Map<AccountResponseDto>(account) : null;
    }

    public async Task DeleteAccountAsync(Guid accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || !account.IsActive)
        {
            throw new KeyNotFoundException($"Account with ID '{accountId}' not found or already inactive.");
        }

        if (account.Balance != 0)
        {
            throw new InvalidOperationException($"Cannot close account '{account.AccountNumber}' with a non-zero balance. Current balance: {account.Balance:C}");
        }

        await _accountRepository.DeleteAsync(accountId);
        await _accountRepository.SaveChangesAsync();
    }

    private string GenerateAccountNumber()
    {
        return $"ACC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
