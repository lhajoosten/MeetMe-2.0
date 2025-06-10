using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Exceptions;

public class Email : ValueObject
{
    public string Value { get; }

    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private const int MaxLength = 320; // Maximum length for an email address per RFC 5321

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Guard.Against.OutOfRange(value.Length, nameof(value), 1, MaxLength);

        if (!EmailRegex.IsMatch(value))
        {
            throw new DomainException($"Invalid email format: {value}");
        }

        return new Email(value.ToLowerInvariant()); // Normalize to lowercase
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}