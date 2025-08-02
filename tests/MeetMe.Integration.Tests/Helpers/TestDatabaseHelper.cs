using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MeetMe.Infrastructure.Data;
using MeetMe.Infrastructure.Services;
using MeetMe.Application.Common.Interfaces;
using MediatR;
using AutoMapper;
using MeetMe.Application.Features.Search.Queries.GlobalSearch;
using MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using MeetMe.Integration.Tests.Infrastructure;

namespace MeetMe.Integration.Tests.Helpers;

public class TestDatabaseHelper : IDisposable
{
    private readonly object _database; // Can be either SqlServerTestDatabase or SharedTestDatabase
    private readonly ServiceProvider _serviceProvider;

    public TestDatabaseHelper(object database)
    {
        _database = database;
        
        var services = new ServiceCollection();
        
        // Add Entity Framework with connection string from either database type
        var connectionString = database switch
        {
            SqlServerTestDatabase sqlDb => sqlDb.GetConnectionString(),
            SharedTestDatabase sharedDb => sharedDb.GetConnectionString(),
            _ => throw new ArgumentException("Invalid database type")
        };
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        // Add AutoMapper
        services.AddAutoMapper(cfg => 
        {
            // Add profiles from the application assembly
        }, typeof(GlobalSearchQuery).Assembly);
        
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GlobalSearchQuery).Assembly));
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add our services
        services.AddScoped<ISearchService, SearchService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    public ApplicationDbContext Context 
    { 
        get 
        {
            return _database switch
            {
                SqlServerTestDatabase sqlDb => sqlDb.GetContext(),
                SharedTestDatabase sharedDb => sharedDb.CreateContext(),
                _ => throw new InvalidOperationException("Invalid database type")
            };
        }
    }
    
    public ISearchService SearchService => _serviceProvider.GetRequiredService<ISearchService>();
    
    public IMediator GetMediator() => _serviceProvider.GetRequiredService<IMediator>();

    public async Task SeedTestDataAsync()
    {
        // Reset database based on type - but only for SqlServerTestDatabase
        // SharedTestDatabase is already cleaned by the caller
        switch (_database)
        {
            case SqlServerTestDatabase sqlDb:
                await sqlDb.ResetAsync();
                break;
            case SharedTestDatabase:
                // Don't clean again - already done by InitializeAsync
                break;
        }
        
        var context = Context;

        // Create test roles first - let SQL Server auto-generate IDs
        var memberRole = new Role { Name = "Member" };
        var adminRole = new Role { Name = "Admin" };
        
        context.Roles.AddRange(memberRole, adminRole);
        await context.SaveChangesAsync();

        // Create test users with roles
        var user1 = User.Create("John", "Doe", "john@example.com", "Software Developer");
        user1.SetPrimaryRole(memberRole);
        
        var user2 = User.Create("Jane", "Smith", "jane@example.com", "Product Manager");
        user2.SetPrimaryRole(memberRole);
        
        var user3 = User.Create("Bob", "Johnson", "bob@example.com", "UX Designer");
        user3.SetPrimaryRole(adminRole);

        context.Users.AddRange(user1, user2, user3);
        await context.SaveChangesAsync();

        // Create test meetings
        var meeting1 = Meeting.Create(
            "Tech Discussion",
            "Weekly tech discussion about new frameworks",
            "Conference Room A",
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow.AddDays(7).AddHours(1),
            user1);

        var meeting2 = Meeting.Create(
            "Product Planning",
            "Monthly product planning session",
            "Meeting Room B",
            DateTime.UtcNow.AddDays(14),
            DateTime.UtcNow.AddDays(14).AddHours(2),
            user2);

        var meeting3 = Meeting.Create(
            "Design Review",
            "Review of new design proposals",
            "Creative Space",
            DateTime.UtcNow.AddDays(21),
            DateTime.UtcNow.AddDays(21).AddHours(1),
            user3);

        var meeting4 = Meeting.Create(
            "Team Standup",
            "Daily team standup meeting",
            "Open Area",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddMinutes(30),
            user1);

        context.Meetings.AddRange(meeting1, meeting2, meeting3, meeting4);
        await context.SaveChangesAsync();

        // Create test posts
        var post1 = Post.Create("Meeting Announcement", "Excited about the tech discussion!", user1, meeting1);
        var post2 = Post.Create("Planning Session", "Looking forward to planning our roadmap", user2, meeting2);
        var post3 = Post.Create("Design Preview", "Can't wait to see the new designs", user3, meeting3);
        var post4 = Post.Create("Meeting Agenda", "Here is the agenda for our upcoming meeting", user1, meeting1);

        context.Posts.AddRange(post1, post2, post3, post4);
        await context.SaveChangesAsync();

        // Create some search queries for analytics
        var searchQuery1 = SearchQuery.Create("tech", "Global", user1.Id, 5, TimeSpan.FromMilliseconds(150));
        var searchQuery2 = SearchQuery.Create("product", "Global", user2.Id, 3, TimeSpan.FromMilliseconds(120));
        var searchQuery3 = SearchQuery.Create("design", "Meeting", user3.Id, 2, TimeSpan.FromMilliseconds(100));
        var searchQuery4 = SearchQuery.Create("tech", "Global", user2.Id, 4, TimeSpan.FromMilliseconds(180));
        var searchQuery5 = SearchQuery.Create("planning", "Post", user1.Id, 1, TimeSpan.FromMilliseconds(90));
        var searchQuery6 = SearchQuery.Create("standup", "Global", user3.Id, 1, TimeSpan.FromMilliseconds(110));

        context.SearchQueries.AddRange(searchQuery1, searchQuery2, searchQuery3, searchQuery4, searchQuery5, searchQuery6);
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
