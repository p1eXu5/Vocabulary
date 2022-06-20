namespace Vocabulary.Categories.Types

open System.Threading.Tasks
open Vocabulary.DataContracts.Types

type ICategoryRepository =
    abstract member GetNavCategoriesAsync : unit -> Task<NavCategory seq>