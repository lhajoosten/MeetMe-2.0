using Microsoft.AspNetCore.Identity;

namespace MeetMe.Domain.Entities
{
    public class MMIdentity : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
