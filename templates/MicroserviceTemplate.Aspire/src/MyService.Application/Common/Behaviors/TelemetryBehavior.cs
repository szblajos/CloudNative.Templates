
using Mediator;
using OpenTelemetry.Trace;

namespace MyService.Application.Common.Behaviors;

public class TelemetryBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly Tracer _tracer;

    public TelemetryBehavior(TracerProvider tracerProvider)
    {
        _tracer = tracerProvider.GetTracer("MyService.Application");
    }

    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {

        using var span = _tracer.StartActiveSpan($"Mediator {typeof(TMessage).Name}");

        try
        {
            // Add custom attributes
            span.SetAttribute("request.type", typeof(TMessage).FullName);

            var response = next(message, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            // Log the exception
            span.RecordException(ex);
            // Add custom attributes
            span.SetAttribute("error.type", ex.GetType().FullName);
            span.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }
}