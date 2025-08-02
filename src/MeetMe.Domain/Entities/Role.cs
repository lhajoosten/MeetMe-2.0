using MeetMe.Domain.Common;

namespace MeetMe.Domain.Entities
{
    public class Role : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
