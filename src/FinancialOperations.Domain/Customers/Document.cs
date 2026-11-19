namespace FinancialOperations.Domain.Customers;

public sealed record Document
{
    public string Value { get; }

    public Document(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 11 || !value.All(char.IsAsciiDigit) || !HasValidCheckDigits(value))
        {
            throw new ArgumentException("Document must be a valid CPF with 11 digits.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;

    private static bool HasValidCheckDigits(string value)
    {
        if (value.All(digit => digit == value[0]))
        {
            return false;
        }

        return CheckDigit(value, 9) == value[9] && CheckDigit(value, 10) == value[10];
    }

    private static char CheckDigit(string value, int length)
    {
        var sum = 0;

        for (var index = 0; index < length; index++)
        {
            sum += (value[index] - '0') * (length + 1 - index);
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;
        return (char)('0' + digit);
    }
}