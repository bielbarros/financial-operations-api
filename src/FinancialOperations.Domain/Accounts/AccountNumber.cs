namespace FinancialOperations.Domain.Accounts;

public sealed record AccountNumber
{
    public string Value { get; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 8 || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Account number must be 8 digits.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}