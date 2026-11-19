using FinancialOperations.Domain.Shared;

namespace FinancialOperations.Domain.Exceptions;

public sealed class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(Money available, Money attempted)
        : base("Debit exceeds the available balance.")
    {
        Available = available;
        Attempted = attempted;
    }

    public Money Available { get; }
    public Money Attempted { get; }
}