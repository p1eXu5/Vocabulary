using MediatR;
using p1eXu5.Result;

namespace Techno.Mir.Upay.Abstractions;

public interface ICommand : IRequest
{ }

public interface ICommand<out TResponse> : IRequest<TResponse>
{ }

/// <summary>
/// <c>TError</c> is string.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IResultCommand<TResponse> : IRequest<Result<TResponse, string>>
{ }

public interface IResultCommand : IRequest<Result<Unit, string>>
{ }
