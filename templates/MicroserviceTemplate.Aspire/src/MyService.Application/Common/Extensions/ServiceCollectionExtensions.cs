using Microsoft.Extensions.DependencyInjection;
using MyService.Application.Items.Validations;
using FluentValidation;
using Mediator;
using MyService.Application.Items.Commands;
using MyService.Application.Items.Mappings;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using MyService.Application.Common.Behaviors;

namespace MyService.Application.Common.Extensions;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds validation services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Returns the configured service collection.</returns>
    public static IServiceCollection AddValidations(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateItemCommandValidator>();

        return services;
    }

    /// <summary>
    /// Adds mapping services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Returns the configured service collection.</returns>
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddSingleton<IItemMapper, ItemMapper>();
        // register other mappers here

        return services;
    }

    /// <summary>
    /// Adds mediator services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Returns the configured service collection.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(CreateItemCommand).Assembly];
        });

        return services;
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        /// Check if Opentelemetry is already configured
        var serviceProvider = services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        if (tracerProvider != null)
        {
            // OpenTelemetry is already configured
            // Add Mediator's pipeline behavior for telemetry
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TelemetryBehavior<,>));
            return services;
        }

        //Setup logging to be exported via OpenTelemetry
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });
        });

        // Add OpenTelemetry
        var otel = services.AddOpenTelemetry();
        // Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource("MyService.Application");
        });

        // Export OpenTelemetry data via OTLP, using env vars for the configuration
        var OtlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (OtlpEndpoint != null)
        {
            otel.UseOtlpExporter(); // Use OTLP exporter if endpoint is configured
            // Add Mediator's pipeline behavior for telemetry
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TelemetryBehavior<,>));
        }
        
        return services;
    }

    /// <summary>
    /// Adds application services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>Returns the configured service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register application-specific services
        services.AddValidations();
        services.AddMappings();
        services.AddMediator();
        services.AddTelemetry(configuration);

        // Register other application services here
        // e.g., services.AddScoped<IItemService, ItemService>();

        return services;
    }
}