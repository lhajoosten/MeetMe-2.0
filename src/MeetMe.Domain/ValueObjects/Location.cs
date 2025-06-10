using Ardalis.GuardClauses;

namespace MeetMe.Domain.ValueObjects
{
    public record Location
    {
        public string Address { get; }
        public string? City { get; }
        public string? Country { get; }
        public double? Latitude { get; }
        public double? Longitude { get; }

        private Location(string address, string? city = null, string? country = null, double? latitude = null, double? longitude = null)
        {
            Guard.Against.NullOrEmpty(address, nameof(address), "Address cannot be null or empty.");
            Guard.Against.Null(latitude, nameof(latitude), "Latitude cannot be null.");
            Guard.Against.Null(longitude, nameof(longitude), "Longitude cannot be null.");

            Address = address;
            City = city;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static Location Create(string address, string? city = null, string? country = null, double? latitude = null, double? longitude = null)
        {
            return new Location(address, city, country, latitude, longitude);
        }

        public string FullAddress => City != null && Country != null
            ? $"{Address}, {City}, {Country}"
            : Address;

        public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

        public override string ToString() => FullAddress;
    }
}