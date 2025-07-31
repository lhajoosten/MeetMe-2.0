using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMe.Infrastructure.Data.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.MeetingId)
                .IsRequired();

            builder.Property(a => a.JoinedAt)
                .IsRequired();

            builder.Property(a => a.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Enum configuration for AttendanceStatus
            builder.HasOne<AttendanceStatus>()
                .WithMany()
                .HasForeignKey("StatusId")
                .IsRequired();

            builder.Property<int>("StatusId")
                .HasColumnName("StatusId");

            builder.Ignore(a => a.Status);

            // Relationships
            builder.HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Meeting)
                .WithMany(m => m.Attendees)
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite unique index to prevent duplicate attendances
            builder.HasIndex(a => new { a.UserId, a.MeetingId })
                .IsUnique();

            builder.HasIndex(a => a.IsActive);
            builder.HasIndex(a => a.JoinedAt);

            // Ignore Domain Events
            builder.Ignore(a => a.DomainEvents);
        }
    }
}
