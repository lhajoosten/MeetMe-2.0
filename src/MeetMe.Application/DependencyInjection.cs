using FluentValidation;
using MediatR;
using MeetMe.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeetMe.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Add FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Add AutoMapper
            services.AddAutoMapper(cfg => 
            {
                // Add profiles from this assembly
            }, Assembly.GetExecutingAssembly());

            // Add MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
