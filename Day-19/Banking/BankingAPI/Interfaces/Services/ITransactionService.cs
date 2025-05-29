public interface ITransactionService
{
    Task<TransactionResponseDto> MakeDepositAsync(MakeDepositDto depositDto);
    Task<TransactionResponseDto> MakeWithdrawalAsync(MakeWithdrawalDto withdrawalDto);
    Task<IEnumerable<TransactionResponseDto>> MakeTransferAsync(MakeTransferDto transferDto); // Returns both transaction records
    Task<IEnumerable<TransactionResponseDto>> GetAccountTransactionsAsync(Guid accountId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<TransactionResponseDto?> GetTransactionByIdAsync(Guid transactionId);
}