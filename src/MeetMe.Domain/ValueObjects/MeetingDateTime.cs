namespace MeetMe.Domain.ValueObjects
{
    public record MeetingDateTime
    {
        public DateTime StartDateTime { get; }
        public DateTime EndDateTime { get; }

        private MeetingDateTime(DateTime startDateTime, DateTime endDateTime)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public static MeetingDateTime Create(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
                throw new ArgumentException("Start date must be before end date");

            if (startDateTime <= DateTime.UtcNow)
                throw new ArgumentException("Meeting cannot be scheduled in the past");

            return new MeetingDateTime(startDateTime, endDateTime);
        }

        public TimeSpan Duration => EndDateTime - StartDateTime;
        public bool IsToday => StartDateTime.Date == DateTime.UtcNow.Date;
        public bool IsUpcoming => StartDateTime > DateTime.UtcNow;
    }
}