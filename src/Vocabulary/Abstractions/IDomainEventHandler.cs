using MediatR;

namespace Techno.Mir.Upay.Abstractions;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : IDomainEvent
{ }
