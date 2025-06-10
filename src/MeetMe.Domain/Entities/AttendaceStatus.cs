using MeetMe.Domain.Common;

namespace MeetMe.Domain.Entities
{
    public class AttendanceStatus : Enumeration 
    {
        public static readonly AttendanceStatus Confirmed = new(1, "Confirmed");
        public static readonly AttendanceStatus Maybe = new(2, "Maybe");
        public static readonly AttendanceStatus NotAttending = new(3, "Not Attending");

        public AttendanceStatus(int id, string name) : base(id, name) { }

        public static IEnumerable<AttendanceStatus> List() => [Confirmed, Maybe, NotAttending];
    }
}