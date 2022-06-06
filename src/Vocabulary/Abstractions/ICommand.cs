using MediatR;
using p1eXu5.Result;

namespace Techno.Mir.Upay.Abstractions;

public interface ICommand : IRequest
{ }


public interface ICommand<out TResponse> : IRequest<TResponse>
{ }

public interface IResultCommand<TResponse> : IRequest<Result<TResponse>>
{ }

public interface IResultCommand : IRequest<Result>
{ }
