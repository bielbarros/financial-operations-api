namespace FinancialOperations.Domain.Customers;

public sealed class Customer
{
    public const int NameMaxLength = 200;

    public Guid Id { get; }
    public string Name { get; }
    public Document Document { get; }
    public string Email { get; }
    public CustomerStatus Status { get; private set; }

    public Customer(string name, Document document, string email)
    {
        Name = NormalizeName(name);
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Email = NormalizeEmail(email);
        Id = Guid.NewGuid();
        Status = CustomerStatus.Active;
    }

    public void Activate()
    {
        Status = CustomerStatus.Active;
    }

    public void Deactivate()
    {
        Status = CustomerStatus.Inactive;
    }

    public override bool Equals(object? obj) => obj is Customer other && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength)
        {
            throw new ArgumentException($"Name is required and must have at most {NameMaxLength} characters.", nameof(name));
        }

        return name.Trim();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var trimmed = email.Trim();
        var at = trimmed.IndexOf('@');

        if (at <= 0 || at != trimmed.LastIndexOf('@') || at == trimmed.Length - 1 || !trimmed[(at + 1)..].Contains('.'))
        {
            throw new ArgumentException("Email must contain a single @ and a domain.", nameof(email));
        }

        return trimmed;
    }
}