using AutoMapper;

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<TransactionResponseDto> MakeDepositAsync(MakeDepositDto depositDto)
    {
        var account = await _accountRepository.GetByIdAsync(depositDto.AccountId); 
        if (account == null || !account.IsActive || account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Account with ID '{depositDto.AccountId}' not found or is not active.");
        }

        account.Balance += depositDto.Amount;
        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = account.AccountId,
            Amount = depositDto.Amount,
            IsDebit = false,
            TransactionType = TransactionType.Deposit,
            TransactionDate = DateTimeOffset.UtcNow,
            Description = depositDto.Description,
            ReferenceNumber = Guid.NewGuid().ToString(),
            Status = TransactionStatus.Completed,
            BalanceAfterTransaction = account.Balance
        };
        await _transactionRepository.AddAsync(transaction);

        await _accountRepository.SaveChangesAsync();

        // Manually assigning account to transaction for correct DTO mapping
        transaction.Account = account;
        return _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task<TransactionResponseDto> MakeWithdrawalAsync(MakeWithdrawalDto withdrawalDto)
    {
        var account = await _accountRepository.GetByIdAsync(withdrawalDto.AccountId); 
        if (account == null || !account.IsActive || account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Account with ID '{withdrawalDto.AccountId}' not found or is not active.");
        }

        if (account.Balance < withdrawalDto.Amount)
        {
            throw new InvalidOperationException($"Insufficient funds in account '{account.AccountNumber}'. Available: {account.Balance:C}, Requested: {withdrawalDto.Amount:C}");
        }

        account.Balance -= withdrawalDto.Amount;
        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = account.AccountId,
            Amount = withdrawalDto.Amount,
            IsDebit = true,
            TransactionType = TransactionType.Withdrawal,
            TransactionDate = DateTimeOffset.UtcNow,
            Description = withdrawalDto.Description,
            ReferenceNumber = Guid.NewGuid().ToString(),
            Status = TransactionStatus.Completed,
            BalanceAfterTransaction = account.Balance
        };
        await _transactionRepository.AddAsync(transaction);

        await _accountRepository.SaveChangesAsync();

        // Manually assigning account to transaction for correct DTO mapping
        transaction.Account = account;
        return _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task<IEnumerable<TransactionResponseDto>> MakeTransferAsync(MakeTransferDto transferDto)
    {
        if (transferDto.SourceAccountId == transferDto.DestinationAccountId)
        {
            throw new InvalidOperationException("Source and destination accounts cannot be the same for a transfer.");
        }

        var sourceAccount = await _accountRepository.GetByIdAsync(transferDto.SourceAccountId); 
        if (sourceAccount == null || !sourceAccount.IsActive || sourceAccount.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Source account with ID '{transferDto.SourceAccountId}' not found or is not active.");
        }

        var destinationAccount = await _accountRepository.GetByIdAsync(transferDto.DestinationAccountId); 
        if (destinationAccount == null || !destinationAccount.IsActive || destinationAccount.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Destination account with ID '{transferDto.DestinationAccountId}' not found or is not active.");
        }

        if (sourceAccount.Balance < transferDto.Amount)
        {
            throw new InvalidOperationException($"Insufficient funds in source account '{sourceAccount.AccountNumber}'. Available: {sourceAccount.Balance:C}, Requested: {transferDto.Amount:C}");
        }

        string sharedReferenceNumber = Guid.NewGuid().ToString();
        DateTimeOffset transactionTime = DateTimeOffset.UtcNow;

        sourceAccount.Balance -= transferDto.Amount;
        await _accountRepository.UpdateAsync(sourceAccount);

        var sourceTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = sourceAccount.AccountId,
            Amount = transferDto.Amount,
            IsDebit = true,
            TransactionType = TransactionType.Transfer,
            TransactionDate = transactionTime,
            Description = $"{transferDto.Description} to {destinationAccount.AccountNumber}",
            ReferenceNumber = sharedReferenceNumber,
            Status = TransactionStatus.Completed,
            BalanceAfterTransaction = sourceAccount.Balance
        };
        await _transactionRepository.AddAsync(sourceTransaction);

        destinationAccount.Balance += transferDto.Amount;
        await _accountRepository.UpdateAsync(destinationAccount);

        var destinationTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = destinationAccount.AccountId,
            Amount = transferDto.Amount,
            IsDebit = false,
            TransactionType = TransactionType.Transfer,
            TransactionDate = transactionTime,
            Description = $"{transferDto.Description} from {sourceAccount.AccountNumber}",
            ReferenceNumber = sharedReferenceNumber,
            Status = TransactionStatus.Completed,
            BalanceAfterTransaction = destinationAccount.Balance
        };
        await _transactionRepository.AddAsync(destinationTransaction);

        await _accountRepository.SaveChangesAsync();

        // Manually assign accounts to transactions for correct DTO mapping
        sourceTransaction.Account = sourceAccount;
        destinationTransaction.Account = destinationAccount;

        var sourceTransactionDto = _mapper.Map<TransactionResponseDto>(sourceTransaction);
        var destinationTransactionDto = _mapper.Map<TransactionResponseDto>(destinationTransaction);

        return new List<TransactionResponseDto> { sourceTransactionDto, destinationTransactionDto };
    }

    public async Task<IEnumerable<TransactionResponseDto>> GetAccountTransactionsAsync(Guid accountId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID '{accountId}' not found.");
        }

        var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId, startDate, endDate); 

        return _mapper.Map<IEnumerable<TransactionResponseDto>>(transactions);
    }

    public async Task<TransactionResponseDto?> GetTransactionByIdAsync(Guid transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return null;

        return _mapper.Map<TransactionResponseDto>(transaction);
    }
}