using Fluxor;
using Fluxor.Blazor.Web.Components;
using Vocabulary.DataContracts.Types;
using Vocabulary.WebClient.Store.Types;
using Vocabulary.WebClient.Store.VocabularyState;

namespace Vocabulary.BlazorServer.Shared;


public partial class NavMenu : FluxorComponent, IDisposable
{
    private bool _grouping = true;

    [Inject] public IDispatcher Dispatcher { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [Inject] private IState<VocabularyState> State { get; set; } = default!;


    private IEnumerable<NavCategory> Categories => State.Value.GetNavCategories();
    private IEnumerable<TermName> UncategorizedTerms => State.Value.GetUncategorizedTerms();
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

