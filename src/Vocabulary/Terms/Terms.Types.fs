namespace Vocabulary.Terms.Types

open System
open System.Threading
open System.Threading.Tasks
open Vocabulary.DataContracts.Types

type TermsInDescription =
    {
        Description: string
        TermNames: TermName list
    }

type ITermRepository =
    abstract member FindTermsInDescriptionAsync : Guid -> CancellationToken -> Task<TermsInDescription option>
    abstract member GetUncategorizedTermsAsync : unit -> Task<TermName seq>
    abstract member GetFullTermsAsync : unit -> Task<FullTerm list>

