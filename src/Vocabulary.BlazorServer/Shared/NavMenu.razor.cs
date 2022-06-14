using Fluxor;
using Fluxor.Blazor.Web.Components;
using Vocabulary.DataContracts.Types;
using Vocabulary.WebClient.Store;

namespace Vocabulary.BlazorServer.Shared;

public partial class NavMenu : FluxorComponent, IDisposable
{
    private bool _grouping = true;

    [Inject] public IDispatcher Dispatcher { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [Inject] private IState<VocabularyState> State { get; set; } = default!;


    private IEnumerable<NavCategory> Categories => VocabularyStateModule.categories(State.Value);
    private IEnumerable<TermName> UncategorizedTerms => VocabularyStateModule.uncategorizedTerms(State.Value);
    private IReadOnlyDictionary<Guid, bool> IsExpanded => State.Value.IsExpandedMap;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(VocabularyStateModule.Msg.StartLoadNavCategoriesOperationMsg());
    }

    private void OnExpandedChanged(Guid id, bool value)
    {
        Dispatcher.Dispatch(VocabularyStateModule.Msg.SetExpandedMsg(id, value));
    }
}

