using MediatR;

namespace Techno.Mir.Upay.Abstractions;

public interface IQuery : IRequest
{ }

public interface IQuery<out TResponse> : IRequest<TResponse>
{ }
