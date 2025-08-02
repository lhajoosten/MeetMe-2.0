using MeetMe.API.Configuration;
using MeetMe.Application;
using MeetMe.Infrastructure;
using MeetMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MeetMe.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetMe.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog early to capture startup logs
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("🚀 Starting MeetMe API...");

                var builder = WebApplication.CreateBuilder(args);

                // Add Serilog
                builder.Host.UseSerilog((context, configuration) =>
                    configuration.ReadFrom.Configuration(context.Configuration));

                // Add services to the container
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // Add application layers
                builder.Services.AddApplication();
                builder.Services.AddInfrastructure(builder.Configuration);

                // Add API configurations
                builder.Services.AddApiConfiguration(builder.Configuration);

                var app = builder.Build();

                // Apply database migrations
                await ApplyDatabaseMigrationsAsync(app);

                // Seed default roles
                await SeedDefaultRolesAsync(app);

                // Configure the HTTP request pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    Log.Information("📚 Swagger UI available at: /index.html");
                }

                app.UseHttpsRedirection();
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                    };
                });

                app.UseCors("CorsPolicy");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                Log.Information("✅ MeetMe API started successfully");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                Log.Information("🔄 Checking database migrations...");
                
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Log.Information("📦 Applying {Count} pending migrations: {Migrations}", 
                        pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                    
                    await context.Database.MigrateAsync();
                    Log.Information("✅ Database migrations applied successfully");
                }
                else
                {
                    Log.Information("✅ Database is up to date");
                }
                
                // Log connection string (masked for security)
                var connectionString = context.Database.GetConnectionString();
                var maskedConnectionString = MaskConnectionString(connectionString);
                Log.Information("🔗 Database connection: {ConnectionString}", maskedConnectionString);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to apply database migrations");
                throw;
            }
        }

        private static async Task SeedDefaultRolesAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                Log.Information("🔄 Checking default roles...");
                
                // Create default roles if they don't exist
                var defaultRoles = new[] { "Member", "Admin" };
                
                foreach (var roleName in defaultRoles)
                {
                    if (!await dbContext.Roles.AnyAsync(r => r.Name == roleName))
                    {
                        var role = new Role { Name = roleName };
                        dbContext.Roles.Add(role);
                        await dbContext.SaveChangesAsync();
                        
                        Log.Information("✅ Created role: {RoleName}", roleName);
                    }
                    else
                    {
                        Log.Information("✅ Role already exists: {RoleName}", roleName);
                    }
                }
                
                Log.Information("✅ Default roles check completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to seed default roles");
                throw;
            }
        }

        private static string MaskConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "Not configured";

            // Mask password in connection string
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"(Password|Pwd)=([^;]*)", 
                "$1=***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
