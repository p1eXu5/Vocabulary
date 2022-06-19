namespace Vocabulary.WebClient.Store

open Vocabulary.DataContracts.Types
open Fluxor
open System.Threading.Tasks
open Elmish.Extensions
open System
open MediatR
open Vocabulary.Terms

type UncategorizedTerms = TermName list


type TermFilter =
    | TermId of Guid
    | Category of Guid


[<FeatureState(CreateInitialStateMethodName="Init")>]
type VocabularyState =
    {
        NavCategories: Deferred<NavCategory list>
        UncategorizedTerms: Deferred<UncategorizedTerms>
        Terms: Deferred<FullTerm list>
        NewTermDescription: Deferred<NewTermDescription>
        CategoryExpanderMap: Map<Guid, bool>
        LastError: string option
    }
    with
        static member Init() =
            {
                NavCategories = Deferred.HasNotBeenRequestedYet
                UncategorizedTerms = Deferred.HasNotBeenRequestedYet
                Terms = Deferred.HasNotBeenRequestedYet
                NewTermDescription = Deferred.HasNotBeenRequestedYet
                CategoryExpanderMap = Map.empty
                LastError = None
            }




module VocabularyState =

    type Msg =
        | LoadNavCategoriesOperation of Operation<unit, (NavCategory list * UncategorizedTerms)>
        | LoadTermsOperation of Operation<unit, FullTerm list>
        | FindLinksInDescriptionOperation of Operation<Guid, NewTermDescription>
        | AcceptNewDescriptionOperation of Operation<unit, unit>
        | ToggleCategoryExpander of id: Guid * value: bool
        | SetError of string
        with
            static member LoadNavCategories = LoadNavCategoriesOperation (Start ())
            static member LoadTerms = LoadTermsOperation (Start ())
            static member FindLinksInDescription(termId) = FindLinksInDescriptionOperation (Start termId)
            static member AcceptNewDescription = AcceptNewDescriptionOperation (Start ())


    [<ReducerMethod>]
    let update model msg =
        match msg with
        // ----------------------------
        //       Initialization
        // ----------------------------
        | LoadNavCategoriesOperation (Start _) ->
            { model with NavCategories = InProgress; UncategorizedTerms = InProgress }

        | LoadNavCategoriesOperation (Finish (categories, uncategorizedTerms)) ->
            let isExpandedMap =
                Guid.Empty :: (categories |> List.map (fun c -> c.Id ))
                |> List.map (fun id -> (id, false))
                |> Map.ofList

            { model with 
                NavCategories = categories |> Deferred.Retrieved; 
                UncategorizedTerms = uncategorizedTerms |> Deferred.Retrieved
                CategoryExpanderMap = isExpandedMap }

        | LoadTermsOperation (Start _) ->
            { model with Terms = InProgress }

        | LoadTermsOperation (Finish terms) ->
            { model with Terms = terms |> Deferred.Retrieved }

        // ----------------------------
        //     NewTermDescription
        // ----------------------------
        | FindLinksInDescriptionOperation (Start _)
        | AcceptNewDescriptionOperation (Start _) ->
            { model with NewTermDescription = InProgress }

        | FindLinksInDescriptionOperation (Finish newTermDescription) ->
            { model with NewTermDescription = newTermDescription |> Deferred.Retrieved }

        | AcceptNewDescriptionOperation (Finish _) ->
            { model with NewTermDescription = Deferred.HasNotBeenRequestedYet }

        // -----------------------------

        | ToggleCategoryExpander (id, v) ->
            { model with CategoryExpanderMap = model.CategoryExpanderMap |> Map.add id v }

        | _ -> model


    let categories model =
        match model.NavCategories with
        | Deferred.InProgress
        | Deferred.HasNotBeenRequestedYet -> []
        | Deferred.Retrieved l -> l


    let uncategorizedTerms model =
        match model.UncategorizedTerms with
        | Deferred.InProgress
        | Deferred.HasNotBeenRequestedYet -> []
        | Deferred.Retrieved l -> l

    let isTermsLoading model =
        match model.Terms with
        | Deferred.InProgress -> true
        | _ -> false

    let terms model =
        match model.Terms with
        | Deferred.InProgress
        | Deferred.HasNotBeenRequestedYet -> []
        | Deferred.Retrieved l -> l



    open System.Threading
    open Vocabulary.Categories.Ports
    open Vocabulary.Terms.Types
    open FsToolkit.ErrorHandling

    type Effects(mediator: IMediator, categoryRepository: ICategoryRepository, termRepository: ITermRepository) =

        [<EffectMethod>]
        member _.Process(msg, dispatcher: IDispatcher) =
            let finish operation result =
                dispatcher.Dispatch(operation <| Operation.Finish result )

            let setError error =
                dispatcher.Dispatch(SetError error)

            task {
                match msg with
                | LoadNavCategoriesOperation (Start _) ->
                    let! categories = categoryRepository.GetNavCategoriesAsync()
                    let! uncategorized = termRepository.GetUncategorizedTermsAsync()
                    do
                        finish LoadNavCategoriesOperation (categories |> Seq.toList, uncategorized |> Seq.toList)

                | LoadTermsOperation (Start _) ->
                    let! fullTerms = termRepository.GetFullTermsAsync()
                    do
                        finish LoadTermsOperation fullTerms

                | FindLinksInDescriptionOperation (Start termId) ->
                    let! newTermDescriptionResult = FindTermsInDescription.exec termRepository termId CancellationToken.None
                    do
                        newTermDescriptionResult
                        |> Option.iter (finish FindLinksInDescriptionOperation)

                | _ ->
                    return ()
            } :> Task
