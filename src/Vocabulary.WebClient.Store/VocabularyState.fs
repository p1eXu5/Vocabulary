namespace Vocabulary.WebClient.Store

open Vocabulary.DataContracts.Types
open Fluxor
open System.Threading.Tasks
open Elmish.Extensions
open Vocabulary.Terms.Ports
open System

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
        CategoryExpanderMap: Map<Guid, bool>
    }
    with
        static member Init() =
            {
                NavCategories = Deferred.HasNotBeenRequestedYet
                UncategorizedTerms = Deferred.HasNotBeenRequestedYet
                Terms = Deferred.HasNotBeenRequestedYet
                CategoryExpanderMap = Map.empty
            }




module VocabularyState =

    type Msg =
        | LoadNavCategoriesOperation of Operation<unit, (NavCategory list * UncategorizedTerms)>
        | ToggleCategoryExpander of id: Guid * value: bool
        | LoadTermsOperation of Operation<unit, FullTerm list>
        with
            static member LoadNavCategoriesMsg () = LoadNavCategoriesOperation (Start ())
            static member SetExpandedMsg(id, value) = ToggleCategoryExpander (id, value)
            static member LoadTermsMsg () = LoadTermsOperation (Start ())


    [<ReducerMethod>]
    let update model msg =
        match msg with
        | LoadNavCategoriesOperation (Start _) when model.NavCategories |> Deferred.hasNotBeenRequested && model.UncategorizedTerms |> Deferred.hasNotBeenRequested ->
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

        | LoadTermsOperation (Start _) when model.Terms |> Deferred.hasNotBeenRequested ->
            { model with Terms = InProgress }

        | LoadTermsOperation (Finish terms) ->
            { model with Terms = terms |> Deferred.Retrieved }

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


    open Vocabulary.Categories.Ports

    type Effects(categoryRepository: ICategoryRepository, termRepository: ITermRepository) =

        [<EffectMethod>]
        member _.Process(msg, dispatcher: IDispatcher) =
            let finish operation result =
                dispatcher.Dispatch(operation <| Operation.Finish result )

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

                | _ ->
                    return ()
            } :> Task
