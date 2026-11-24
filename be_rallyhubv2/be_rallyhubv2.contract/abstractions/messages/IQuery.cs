using be_rallyhubv2.contract.abstractions.results;
using MediatR;

namespace be_rallyhubv2.contract.abstractions.messages;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;