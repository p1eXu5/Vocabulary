namespace Vocabulary.DataContracts.Types

open System


type SynonymVm =
    {
        Name: string
    }

type LinkVm =
    {
        ResourceDescription: string
        Href: string
    }


type FullTerm =
    {
        Id: Guid
        Name: string
        AdditionalName: string
        Description: string
        ValidationRules: string
        Synonyms: SynonymVm list
        CategoryIds: Guid list
        Links: LinkVm list
    }

type TermName =
    {
        Id: Guid
        Name: string
        AdditionalName: string option
    }
    with
        member this.HasAdditionalName with get() = this.AdditionalName |> Option.isSome

type NavCategory =
    {
        Id: Guid
        Name: string
        TermNames: TermName list
    }


module NavCategory =
    
    let create id name termNames =
        {
            Id = id
            Name = name
            TermNames = termNames |> Seq.toList
        }

module TermName =

    let create id name additionalName =
        {
            Id = id
            Name = name
            AdditionalName =
                match additionalName with
                | null -> None
                | _ -> additionalName |> Some
        }