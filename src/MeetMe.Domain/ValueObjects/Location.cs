using Ardalis.GuardClauses;
using MeetMe.Domain.Common;

namespace MeetMe.Domain.ValueObjects
{
    public class Location : ValueObject
    {
        public string Value { get; }

        private const int MaxLength = 500;

        private Location(string value)
        {
            Value = value;
        }

        public static Location Create(string value)
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(value));
            Guard.Against.OutOfRange(value.Length, nameof(value), 1, MaxLength);

            return new Location(value.Trim());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Location location) => location.Value;
    }
}