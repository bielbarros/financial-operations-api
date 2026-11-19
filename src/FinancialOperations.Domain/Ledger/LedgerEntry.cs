using FinancialOperations.Domain.Shared;

namespace FinancialOperations.Domain.Ledger;

public sealed class LedgerEntry
{
    public const int DescriptionMaxLength = 200;

    public Guid Id { get; }
    public Money Amount { get; }
    public EntryDirection Direction { get; }
    public DateTimeOffset OccurredAt { get; }
    public string Description { get; }

    internal LedgerEntry(Money amount, EntryDirection direction, DateTimeOffset occurredAt, string description)
    {
        if (amount.Amount == 0)
        {
            throw new ArgumentException("Entry amount must be positive.", nameof(amount));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException("Occurred at is required.", nameof(occurredAt));
        }

        Id = Guid.NewGuid();
        Amount = amount;
        Direction = direction;
        OccurredAt = occurredAt;
        Description = NormalizeDescription(description);
    }

    public override bool Equals(object? obj) => obj is LedgerEntry other && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Description is required and must have at most {DescriptionMaxLength} characters.", nameof(description));
        }

        return description.Trim();
    }
}