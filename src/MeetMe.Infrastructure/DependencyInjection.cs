using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Services;
using MeetMe.Domain.Entities;
using MeetMe.Infrastructure.Data;
using MeetMe.Infrastructure.Repositories;
using MeetMe.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MeetMe.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Add Identity
            services.AddIdentityCore<MMIdentity>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add Repositories
            services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            services.AddScoped(typeof(IQueryRepository<,>), typeof(QueryRepository<,>));
            services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
            services.AddScoped(typeof(ICommandRepository<,>), typeof(CommandRepository<,>));

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add Application Services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordService, PasswordService>();

            return services;
        }
    }
}
