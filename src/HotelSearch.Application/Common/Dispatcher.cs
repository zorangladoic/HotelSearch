using FluentValidation;
using HotelSearch.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Application.Common;

public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        // Run validation if validators exist
        await ValidateAsync(request, requestType, cancellationToken);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {requestType.Name}");

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");

        var result = handleMethod.Invoke(handler, [request, cancellationToken]);

        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {requestType.Name} did not return expected Task<{typeof(TResponse).Name}>");
    }

    private async Task ValidateAsync(object request, Type requestType, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validators = _serviceProvider.GetServices(validatorType).ToList();

        if (validators.Count == 0)
        {
            return;
        }

        // Create ValidationContext using reflection for the correct type
        var contextType = typeof(ValidationContext<>).MakeGenericType(requestType);
        var context = Activator.CreateInstance(contextType, request);

        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            // Get the ValidateAsync method
            var validateMethod = validatorType.GetMethod("ValidateAsync", [contextType, typeof(CancellationToken)]);
            if (validateMethod != null)
            {
                var resultTask = validateMethod.Invoke(validator, [context, cancellationToken]);
                if (resultTask is Task<FluentValidation.Results.ValidationResult> task)
                {
                    var validationResult = await task;
                    failures.AddRange(validationResult.Errors);
                }
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}
