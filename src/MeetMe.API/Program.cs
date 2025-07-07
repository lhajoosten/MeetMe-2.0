using MeetMe.API.Configuration;
using MeetMe.Application;
using MeetMe.Infrastructure;
using Serilog;

namespace MeetMe.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
