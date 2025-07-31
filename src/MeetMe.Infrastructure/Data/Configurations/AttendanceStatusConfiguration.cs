using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMe.Infrastructure.Data.Configurations
{
    public class AttendanceStatusConfiguration : IEntityTypeConfiguration<AttendanceStatus>
    {
        public void Configure(EntityTypeBuilder<AttendanceStatus> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever(); // We'll specify the values manually

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Seed data
            builder.HasData(
                AttendanceStatus.Confirmed,
                AttendanceStatus.Maybe,
                AttendanceStatus.NotAttending
            );

            // Index
            builder.HasIndex(s => s.Name)
                .IsUnique();
        }
    }
}
