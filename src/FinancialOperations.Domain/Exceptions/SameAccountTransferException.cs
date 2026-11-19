using FinancialOperations.Domain.Accounts;

namespace FinancialOperations.Domain.Exceptions;

public sealed class SameAccountTransferException : DomainException
{
    public SameAccountTransferException(AccountNumber accountNumber)
        : base("Transfer requires two distinct accounts.")
    {
        AccountNumber = accountNumber;
    }

    public AccountNumber AccountNumber { get; }
}