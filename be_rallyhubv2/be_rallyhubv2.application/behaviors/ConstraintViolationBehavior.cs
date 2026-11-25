using be_rallyhubv2.contract.abstractions.common;
using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.contract.services.booking;
using be_rallyhubv2.contract.services.identity;
using be_rallyhubv2.domain.exceptions;
using MediatR;

namespace be_rallyhubv2.application.behaviors;

public sealed class ConstraintViolationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result

{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }

        catch (ConstraintViolationException ex) when (ConstraintErrorMap.TryGet(ex.ConstraintName, out var error))
        {
            return ResultFactory.Failure<TResponse>(error);
        }
        catch (ConcurrencyConflictException)
        {
            return ResultFactory.Failure<TResponse>(CommonErrors.ConcurrencyConflict);
        }
    }
}