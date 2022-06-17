using MediatR;

namespace Techno.Mir.Upay.Abstractions;

public interface IQueryHandler : IRequestHandler<IQuery>
{ }

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{ }
