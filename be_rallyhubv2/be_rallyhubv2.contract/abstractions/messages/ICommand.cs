using be_rallyhubv2.contract.abstractions.results;
using MediatR;

namespace be_rallyhubv2.contract.abstractions.messages;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
