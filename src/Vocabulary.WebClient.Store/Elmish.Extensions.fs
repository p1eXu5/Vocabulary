module Elmish.Extensions

type Deferred<'t> =
    | HasNotBeenRequestedYet
    | InProgress
    | Retrieved of 't

/// <summary>
/// see <see href="https://zaid-ajaj.github.io/the-elmish-book/#/chapters/commands/async-state"/>
/// </summary>
type Operation<'TArg, 'TRes> =
    | Start of 'TArg
    | Finish of 'TRes


module Deferred =

    let value = function
        | Retrieved v -> v |> Some
        | _ -> None

    let (|Retrieved|_|) = value

    let hasNotBeenRequested = function
        | HasNotBeenRequestedYet -> true
        | _ -> false
