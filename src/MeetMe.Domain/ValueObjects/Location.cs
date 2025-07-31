using Ardalis.GuardClauses;

namespace MeetMe.Domain.ValueObjects
{
    public record Location
    {
        public string Value { get; }

        private Location(string value)
        {
            Guard.Against.NullOrEmpty(value, nameof(value), "Location cannot be null or empty.");
            Value = value;
        }

        public static Location Create(string location)
        {
            return new Location(location);
        }

        public override string ToString() => Value;

        public static implicit operator string(Location location) => location.Value;
    }
}