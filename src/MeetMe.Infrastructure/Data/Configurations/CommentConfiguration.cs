using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMe.Infrastructure.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.AuthorId)
                .IsRequired();

            builder.Property(c => c.PostId)
                .IsRequired();

            builder.Property(c => c.ParentCommentId)
                .IsRequired(false);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing relationship for comment replies
            builder.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.AuthorId);
            builder.HasIndex(c => c.PostId);
            builder.HasIndex(c => c.ParentCommentId);
            builder.HasIndex(c => c.CreatedDate);

            // Ignore Domain Events
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
