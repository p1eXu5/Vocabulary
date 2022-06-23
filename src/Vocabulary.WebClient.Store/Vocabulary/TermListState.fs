namespace Vocabulary.WebClient.Store.Types

open Elmish.Extensions
open Vocabulary.DataContracts.Types

type TermListState =
    {
        Terms: Deferred<TermState list>
        NewTermDescription: Deferred<NewTermDescription>
    }
    with
        static member Init() =
            {
                Terms = Deferred.HasNotBeenRequestedYet
                NewTermDescription = Deferred.HasNotBeenRequestedYet
            }

        member this.IsTermsLoading =
            match this.Terms with
            | Deferred.InProgress -> true
            | _ -> false

        member this.GetTerms() =
            match this.Terms with
            | Deferred.InProgress
            | Deferred.HasNotBeenRequestedYet -> []
            | Deferred.Retrieved l -> l |> List.map (fun t -> t.Original)


namespace Vocabulary.WebClient.Store.TermListState

open Vocabulary.DataContracts.Types
open Fluxor
open System.Threading.Tasks
open Elmish.Extensions
open System
open MediatR
open Vocabulary.Terms
open Vocabulary.Terms.Types
open Vocabulary.WebClient.Store.Types
open System.Threading

type Msg =
    | LoadTermsOperation of Operation<unit, FullTerm list>
    | FindLinksInDescriptionOperation of Operation<Guid, NewTermDescription>
    | AcceptNewDescriptionOperation of Operation<Guid, unit>
    | CancelNewDescriptionFlow

    with
        static member LoadTerms = LoadTermsOperation (Start ())

        static member FindLinksInDescription(termId) = 
            Start termId |> Msg.FindLinksInDescriptionOperation
            
        static member AcceptNewDescription(termId) =
            Start termId |> Msg.AcceptNewDescriptionOperation


module Program =

    let update model msg =
        match msg with
        | LoadTermsOperation (Start _) ->
            { model with Terms = InProgress }

        | LoadTermsOperation (Finish terms) ->
            { model with 
                Terms = 
                    terms 
                    |> List.map TermState.Init
                    |> Deferred.Retrieved }

        | FindLinksInDescriptionOperation (Start _)
        | AcceptNewDescriptionOperation (Start _) ->
            { model with NewTermDescription = InProgress }

        | FindLinksInDescriptionOperation (Finish newTermDescription) ->
            { model with NewTermDescription = newTermDescription |> Deferred.Retrieved }

        | CancelNewDescriptionFlow
        | AcceptNewDescriptionOperation (Finish _) ->
            { model with NewTermDescription = Deferred.HasNotBeenRequestedYet }

        //| TermStateMsg (termId, termMsg) ->
        //    model.Terms
        //    |> Deferred.value
        //    |> Option.map (fun terms ->
        //        terms
        //        |> List.map (fun t -> 
        //            if t.Id = termId then
        //                TermState.update t termMsg
        //            else
        //                t
        //        )
        //        |> (fun l ->
        //            { model with Terms = l |> Deferred.Retrieved }
        //        )
        //    )
        //    |> Option.defaultValue model


    let ``process`` (termRepository: ITermRepository) msg =
        task {
            match msg with
            | LoadTermsOperation (Start _) ->
                let! fullTerms = termRepository.GetFullTermsAsync()
                return
                    fullTerms |> Operation.Finish |> LoadTermsOperation |> Some

            | FindLinksInDescriptionOperation (Start termId) ->
                let! newTermDescriptionOpt = FindTermsInDescription.exec termRepository termId CancellationToken.None
                return
                    newTermDescriptionOpt
                    |> Option.map ( Operation.Finish >> FindLinksInDescriptionOperation)
                    |> Option.orElse (Some CancelNewDescriptionFlow)

            | _ -> return None
        }