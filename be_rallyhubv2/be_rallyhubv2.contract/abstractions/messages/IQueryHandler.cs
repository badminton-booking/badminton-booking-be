using be_rallyhubv2.contract.abstractions.results;
using MediatR;

namespace be_rallyhubv2.contract.abstractions.messages;

public interface IQueryHandler<in TQuery, TResponse>: IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;