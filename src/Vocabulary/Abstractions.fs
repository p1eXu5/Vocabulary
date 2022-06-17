namespace Vocabulary.Abstractions

open MediatR

type ICommand<'TResponse, 'TError> =
    inherit IRequest<Result<'TResponse, 'TError>>


type ICommandHandler<'TCommand, 'TResponse, 'TError  when 'TCommand :> ICommand<'TResponse, 'TError>> =
    inherit IRequestHandler<'TCommand, Result<'TResponse, 'TError>>
