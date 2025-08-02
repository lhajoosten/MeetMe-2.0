using FluentValidation;
using MediatR;
using MeetMe.Application.Common.Behaviors;
using MeetMe.Application.Common.Mappings;
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

            // Add AutoMapper with automatic profile discovery
            services.AddAutoMapper(cfg => 
            {
                cfg.AddProfile<MappingProfile>();
            }, Assembly.GetExecutingAssembly());

            // Add MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
