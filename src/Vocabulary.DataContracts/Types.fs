namespace Vocabulary.DataContracts.Types

open System

// --------
// Synonym
// --------

type Synonym =
    {
        Name: string
    }

// --------
// Link
// --------

type Link =
    {
        ResourceDescription: string
        Href: string
    }


// --------
// FullTerm
// --------

type FullTerm =
    {
        Id: Guid
        Name: string
        AdditionalName: string
        Description: string
        ValidationRules: string
        Synonyms: Synonym list
        CategoryIds: Guid list
        Links: Link list
    }

module FullTerm =

    let create id name additionalName description validationRule synonyms categoryIds links =
        {
            Id = id
            Name = name
            AdditionalName = additionalName
            Description = description
            ValidationRules = validationRule
            Synonyms = synonyms |> Seq.toList
            CategoryIds = categoryIds |> Seq.toList
            Links = links |> Seq.toList
        }

    let toFSharpList fullTerms : FullTerm list =
        fullTerms |> Seq.toList


// --------
// TermName
// --------

type TermName =
    {
        Id: Guid
        Name: string
        AdditionalName: string option
    }
    with
        member this.HasAdditionalName with get() = this.AdditionalName |> Option.isSome


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


// -----------
// NavCategory
// -----------

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


