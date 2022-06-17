namespace Vocabulary.Terms.FindTermsInDescription


open System
open System.Threading

open FsToolkit.ErrorHandling

open Vocabulary.Abstractions
open Vocabulary.Terms.Types
open Vocabulary.DataContracts.Types


[<RequireQualifiedAccess>]
module Command =
    let internal checkTerms termsInDescription =
        Error "Not implemented"

    let exec (termRepository: ITermRepository) termId cancellationToken =
        taskResult {
            let! termsInDescription = termRepository.FindTermsInDescriptionAsync termId cancellationToken
            return! checkTerms termsInDescription
        }




type Command =
    {
        TermId: Guid
    }
    interface ICommand<NewTermDescription, string>


type FindTermsInDescriptionCommandHandler(termRepository: ITermRepository) =
    interface ICommandHandler<Command, NewTermDescription, string> with
        member _.Handle(command: Command, cancellationToken: CancellationToken) =
            taskResult {
                return! Command.exec termRepository command.TermId cancellationToken
            }