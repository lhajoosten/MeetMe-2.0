using MeetMe.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetMe.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<MMIdentity, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<MMIdentity> MMIdentities => Set<MMIdentity>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<SearchQuery> SearchQueries => Set<SearchQuery>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}