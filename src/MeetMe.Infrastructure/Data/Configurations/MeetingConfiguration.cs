using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMe.Infrastructure.Data.Configurations
{
    public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            // Primary key
            builder.HasKey(m => m.Id);

            // Properties
            builder.Property(m => m.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(m => m.Description)
                .HasMaxLength(2000);

            // Value Object mapping for Location
            builder.OwnsOne(m => m.Location, location =>
            {
                location.Property(l => l.Value)
                    .HasColumnName("Location")
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // Value Object mapping for MeetingDateTime
            builder.OwnsOne(m => m.MeetingDateTime, dateTime =>
            {
                dateTime.Property(d => d.StartDateTime)
                    .HasColumnName("StartDateTime")
                    .IsRequired();

                dateTime.Property(d => d.EndDateTime)
                    .HasColumnName("EndDateTime")
                    .IsRequired();
            });

            // Relationships
            builder.HasOne(m => m.Creator)
                .WithMany(u => u.CreatedMeetings)
                .HasForeignKey(m => m.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.Attendees)
                .WithOne(a => a.Meeting)
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Posts)
                .WithOne(p => p.Meeting)
                .HasForeignKey(p => p.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(m => m.IsActive);
            builder.HasIndex(m => m.CreatorId);
            builder.HasIndex(m => m.CreatedDate);

            // Ignore Domain Events (they are not persisted)
            builder.Ignore(m => m.DomainEvents);
        }
    }
}
