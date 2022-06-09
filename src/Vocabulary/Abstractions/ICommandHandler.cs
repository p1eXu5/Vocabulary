using MediatR;
using p1eXu5.Result;

namespace Techno.Mir.Upay.Abstractions;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> where TCommand : ICommand
{ }

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{ }

public interface IResultCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : IResultCommand<TResponse>
{ }

public interface IResultCommandHandler<TCommand> : IRequestHandler<TCommand, Result> where TCommand : IResultCommand
{ }

