module Elmish.Extensions

type Deferred<'t> =
    | HasNotStartedYet
    | InProgress
    | Resolved of 't

/// <summary>
/// see <see href="https://zaid-ajaj.github.io/the-elmish-book/#/chapters/commands/async-state"/>
/// </summary>
type Operation<'TArg, 'TRes> =
    | Start of 'TArg
    | Finish of 'TRes


module Deferred =

    let isNotStarted = function
        | HasNotStartedYet -> true
        | _ -> false