namespace Vocabulary.WebClient.Store.Types

open System
open Elmish.Extensions
open Vocabulary.DataContracts.Types


type TermState =
    {
        Id: Guid
        Original: FullTerm
        
    }
    with
    static member Init(fullTerm: FullTerm) =
        {
            Id = fullTerm.Id
            Original = fullTerm
        }


namespace Vocabulary.WebClient.Store.TermState

open Vocabulary.DataContracts.Types
open Fluxor
open System.Threading.Tasks
open Elmish.Extensions
open System
open MediatR
open Vocabulary.Terms
open Vocabulary.Terms.Types
open Vocabulary.WebClient.Store.Types


type Msg =
    | No
