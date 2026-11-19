using FinancialOperations.Domain.Accounts;

namespace FinancialOperations.Domain.Exceptions;

public sealed class InactiveAccountException : DomainException
{
    public InactiveAccountException(AccountStatus status)
        : base("Only an active account can move money.")
    {
        Status = status;
    }

    public AccountStatus Status { get; }
}