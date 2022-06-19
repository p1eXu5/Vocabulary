namespace Vocabulary.Terms

[<RequireQualifiedAccess>]
module FindTermsInDescription =
    
    open System
    open System.Runtime
    open FsToolkit.ErrorHandling
    open Vocabulary.Terms.Types
    open Vocabulary.DataContracts.Types


    type internal Replacement =
        {
            OriginInd: int
            Term: string
            Link: string
            Difference: int
        }

    let internal checkTerms (termsInDescription: TermsInDescription) =
        let tryCreateReplacement (termName: string) =
            let description = termsInDescription.Description
            let ind = description.IndexOf(termName)

            if
                ind >= 0
                &&
                (
                    (ind + termName.Length) = description.Length 
                    || Char.IsWhiteSpace(description[ind + termName.Length])
                    || Char.IsPunctuation(description[ind + termName.Length])
                )
            then
                let link = $"[{termName}](/terms?search={Uri.EscapeDataString(termName)})";
                {
                    OriginInd = ind
                    Term = termName
                    Link = link
                    Difference = link.Length - termName.Length
                } |> Some
            else
                None


        let validReplacement res replacement =
            match res with
            | [] -> replacement :: res
            | lastReplacement :: _ ->
                if (lastReplacement.OriginInd + lastReplacement.Term.Length) < replacement.OriginInd then
                    replacement :: res
                else
                    res            


        termsInDescription.TermNames
        |> List.map (fun tn -> tn.Name)
        |> List.choose tryCreateReplacement
        |> List.sortBy (fun r -> r.OriginInd)
        |> List.sortByDescending (fun r -> r.Difference)
        |> List.distinctBy (fun r -> r.OriginInd)
        |> List.fold validReplacement []
        |> List.rev
        |> (function [] -> None | l -> (termsInDescription.Description, l) |> Some)


    let internal createNewTermDescription (description: string) (replacements: Replacement list) =
        let state =
            {|
                DescriptionMemory = description.AsMemory()
                BuffLength = (replacements |> List.sumBy(fun r -> r.Difference)) + description.Length
                StartOrigin = 0
                StartRes = 0
                OriginLength = 0
            |}

        let folder 
            (state: {| 
                DescriptionMemory: ReadOnlyMemory<char>; 
                BuffLength: int; 
                StartOrigin: int; 
                StartRes: int; 
                OriginLength: int;
                Buffer: char array; 
                Result: Memory<char> |}) 
            replacement
            =
            if state.StartOrigin < replacement.OriginInd then
                let originLength = replacement.OriginInd - state.StartOrigin
                let resSpan = state.Result.Slice(state.StartRes, originLength)
                state.DescriptionMemory
                    .Slice(state.StartOrigin, originLength)
                    .CopyTo(resSpan)

                let startRes = state.StartRes + originLength
                let resSpan = state.Result.Slice(startRes, replacement.Link.Length)
                replacement.Link.AsMemory().CopyTo(resSpan)
                {| state with
                    StartRes = startRes + replacement.Link.Length
                    StartOrigin = replacement.OriginInd + replacement.Term.Length |}
            else
                state

        let buffer : char array = ' ' |> Array.replicate 10

        replacements
        |> List.fold 
            folder
            {|
                DescriptionMemory = description.AsMemory()
                BuffLength = (replacements |> List.sumBy(fun r -> r.Difference)) + description.Length
                StartOrigin = 0
                StartRes = 0
                OriginLength = 0
                Buffer = buffer
                Result = Memory<char>(buffer)
            |}
        |> (fun state ->
            let originLength = description.Length - state.StartOrigin
            let resSpan = state.Result.Slice(state.StartRes, originLength)
            state.DescriptionMemory
                .Slice(state.StartOrigin, originLength)
                .CopyTo(resSpan)

            String(state.Buffer)
        )


    let exec (termRepository: ITermRepository) termId cancellationToken =
        task {
            let! termsInDescription = termRepository.FindTermsInDescriptionAsync termId cancellationToken
            return 
                termsInDescription
                |> Option.bind checkTerms 
                |> Option.map ((<||) createNewTermDescription)
                |> Option.map (fun newDescription ->
                    {
                        TermId = termId
                        Description = newDescription
                    }
                )
        }
