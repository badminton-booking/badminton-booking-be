using be_rallyhubv2.contract.abstractions.results;
using MediatR;

namespace be_rallyhubv2.contract.abstractions.messages;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
