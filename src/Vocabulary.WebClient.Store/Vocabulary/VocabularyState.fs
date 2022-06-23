namespace Vocabulary.WebClient.Store.Types

open System
open Fluxor
open Elmish.Extensions
open Vocabulary.DataContracts.Types

type UncategorizedTerms = TermName list


type TermFilter =
    | TermId of Guid
    | Category of Guid


type Page =
    | TermList of TermListState


[<FeatureState(CreateInitialStateMethodName="Init")>]
type VocabularyState =
    {
        NavCategories: Deferred<NavCategory list>
        UncategorizedTerms: Deferred<UncategorizedTerms>
        CategoryExpanderMap: Map<Guid, bool>
        LastError: string option
        TermListState: TermListState
    }
    with
        static member Init() =
            {
                NavCategories = Deferred.HasNotBeenRequestedYet
                UncategorizedTerms = Deferred.HasNotBeenRequestedYet
                CategoryExpanderMap = Map.empty
                LastError = None
                TermListState = TermListState.Init()
            }
        
        member this.GetNavCategories() =
            match this.NavCategories with
            | Deferred.InProgress
            | Deferred.HasNotBeenRequestedYet -> []
            | Deferred.Retrieved l -> l

        member this.GetUncategorizedTerms() =
            match this.UncategorizedTerms with
            | Deferred.InProgress
            | Deferred.HasNotBeenRequestedYet -> []
            | Deferred.Retrieved l -> l


namespace Vocabulary.WebClient.Store.VocabularyState

open System
open System.Threading.Tasks

open Fluxor
open Elmish.Extensions

open Vocabulary.Terms
open Vocabulary.DataContracts.Types
open Vocabulary.WebClient.Store
open Vocabulary.WebClient.Store.Types

type Msg =
    | LoadNavCategoriesOperation of Operation<unit, (NavCategory list * UncategorizedTerms)>
    | ToggleCategoryExpander of id: Guid * value: bool
    | SetError of string
    | TermListMsg of TermListState.Msg
    with
        static member LoadNavCategories = LoadNavCategoriesOperation (Start ())

module Program =

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

        | ToggleCategoryExpander (id, v) ->
            { model with CategoryExpanderMap = model.CategoryExpanderMap |> Map.add id v }

        | TermListMsg termListMsg ->
            let termListState = TermListState.Program.update model.TermListState termListMsg
            { model with TermListState = termListState }


        | _ -> model


open System.Threading
open Vocabulary.Categories.Types
open Vocabulary.Terms.Types
open FsToolkit.ErrorHandling

type Effects(categoryRepository: ICategoryRepository, termRepository: ITermRepository) =

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

            | TermListMsg termListMsg ->
                let! termListMsg' = 
                    TermListState.Program.``process`` termRepository termListMsg

                do
                    termListMsg' 
                    |> Option.iter (TermListMsg >> dispatcher.Dispatch)

            | _ ->
                return ()
        } :> Task
