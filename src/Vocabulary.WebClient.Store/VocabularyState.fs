namespace Vocabulary.WebClient.Store

open Vocabulary.DataContracts.Types
open Fluxor
open System.Threading.Tasks
open Elmish.Extensions
open Vocabulary.Terms.Ports
open System

type UncategorizedTerms = TermName list

[<FeatureState(CreateInitialStateMethodName="Init")>]
type VocabularyState =
    {
        NavCategories: Deferred<NavCategory list>
        UncategorizedTerms: Deferred<UncategorizedTerms>
        IsExpandedMap: Map<Guid, bool>
    }
    with
        static member Init() =
            {
                NavCategories = Deferred.HasNotStartedYet
                UncategorizedTerms = Deferred.HasNotStartedYet
                IsExpandedMap = Map.empty
            }


module VocabularyState =

    type Msg =
        | LoadNavCategoriesOperation of Operation<unit, (NavCategory list * UncategorizedTerms)>
        | SetExpanded of id: Guid * value: bool
        with
            static member StartLoadNavCategoriesOperationMsg () =
                LoadNavCategoriesOperation (Start ())

            static member SetExpandedMsg(id, value) =
                SetExpanded (id, value)


    [<ReducerMethod>]
    let update model msg =
        match msg with
        | LoadNavCategoriesOperation (Start _) when model.NavCategories |> Deferred.isNotStarted && model.UncategorizedTerms |> Deferred.isNotStarted ->
            { model with NavCategories = InProgress; UncategorizedTerms = InProgress }

        | LoadNavCategoriesOperation (Finish (categories, uncategorizedTerms)) ->
            let isExpandedMap =
                Guid.Empty :: (categories |> List.map (fun c -> c.Id ))
                |> List.map (fun id -> (id, false))
                |> Map.ofList

            { model with 
                NavCategories = categories |> Deferred.Resolved; 
                UncategorizedTerms = uncategorizedTerms |> Deferred.Resolved
                IsExpandedMap = isExpandedMap }

        | SetExpanded (id, v) ->
            { model with IsExpandedMap = model.IsExpandedMap |> Map.add id v }

        | _ -> model


    let categories model =
        match model.NavCategories with
        | Deferred.InProgress
        | Deferred.HasNotStartedYet -> []
        | Deferred.Resolved l -> l


    let uncategorizedTerms model =
        match model.UncategorizedTerms with
        | Deferred.InProgress
        | Deferred.HasNotStartedYet -> []
        | Deferred.Resolved l -> l


    open Vocabulary.Categories.Ports

    type Effects(categoryRepository: ICategoryRepository, termRepository: ITermRepository) =

        [<EffectMethod>]
        member _.Process(msg, dispatcher: IDispatcher) =
            task {
                match msg with
                | LoadNavCategoriesOperation (Start _) ->
                    let! categories = categoryRepository.GetNavCategoriesAsync()
                    let! uncategorized = termRepository.GetUncategorizedTermsAsync()
                    do
                        dispatcher.Dispatch(Operation.Finish (categories |> Seq.toList, uncategorized |> Seq.toList) |> LoadNavCategoriesOperation)
                | _ ->
                    return ()
            } :> Task
