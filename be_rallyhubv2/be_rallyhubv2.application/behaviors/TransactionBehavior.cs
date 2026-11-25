using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.contract.abstractions.messages;
using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.domain.abstractions.repositories;
using MediatR;

namespace be_rallyhubv2.application.behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IPostCommitActionQueue postCommitActions): IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        
        TResponse response;
        
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            response = await next();

            if (response.IsSuccess)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        if (response.IsFailure)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return response;
        }
        
        await postCommitActions.RunAllAsync(CancellationToken.None);
        return response;
    }
}