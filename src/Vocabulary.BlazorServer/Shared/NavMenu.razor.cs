using Fluxor;
using Fluxor.Blazor.Web.Components;
using Vocabulary.DataContracts.Types;
using Vocabulary.WebClient.Store;

namespace Vocabulary.BlazorServer.Shared;

using Msg = VocabularyStateModule.Msg;

public partial class NavMenu : FluxorComponent, IDisposable
{
    private bool _grouping = true;

    [Inject] public IDispatcher Dispatcher { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [Inject] private IState<VocabularyState> State { get; set; } = default!;


    private IEnumerable<NavCategory> Categories => VocabularyStateModule.categories(State.Value);
    private IEnumerable<TermName> UncategorizedTerms => VocabularyStateModule.uncategorizedTerms(State.Value);
    private IReadOnlyDictionary<Guid, bool> IsExpanded => State.Value.CategoryExpanderMap;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(Msg.LoadNavCategories);
    }

    private void OnExpandedChanged(Guid id, bool value)
    {
        Dispatcher.Dispatch(Msg.NewToggleCategoryExpander(id, value));
    }
}

