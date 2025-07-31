using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMe.Infrastructure.Data.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(p => p.Content)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(p => p.AuthorId)
                .IsRequired();

            builder.Property(p => p.MeetingId)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Meeting)
                .WithMany(m => m.Posts)
                .HasForeignKey(p => p.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.AuthorId);
            builder.HasIndex(p => p.MeetingId);
            builder.HasIndex(p => p.CreatedDate);

            // Ignore Domain Events
            builder.Ignore(p => p.DomainEvents);
        }
    }
}
