using be_rallyhubv2.contract.abstractions.results;
using FluentValidation;
using MediatR;

namespace be_rallyhubv2.application.behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validatorList = validators.ToArray();
        if (validatorList.Length == 0)
            return await next();
        
        var context = new ValidationContext<TRequest>(request);
        var result = await Task.WhenAll(validatorList.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        
        var failures = result
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}