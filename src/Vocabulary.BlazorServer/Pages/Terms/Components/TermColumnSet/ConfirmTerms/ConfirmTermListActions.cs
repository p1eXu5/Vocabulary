using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Categories.Ports;

namespace Vocabulary.BlazorServer.Pages.Terms.Components.TermColumnSet.ConfirmTerms;

public abstract class ConfirmTermListActions : ComponentBase
{
    [Parameter]
    public string BroadestColumnMinWidth { get; set; } = "";

    [Parameter]
    public EventCallback<string> NavigateToEdit { get; set; }

    [Parameter]
    public EventCallback<Guid> IsNotInDbChanged { get; set; }

    [Parameter]
    public EventCallback<(Guid, IEnumerable<Category>)> SelectedCategoriesChanged { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; private set; } = default!;

    [Inject]
    private ICategoryRepository CategoryRepository { get; set; } = default!;


    protected ImmutableArray<Category> _categories = ImmutableArray<Category>.Empty;


    protected override Task OnParametersSetAsync()
    {
        return LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _categories = await CategoryRepository.GetCategoriesAsync();
    }


    protected async Task OnNavigateToEdit(string termId)
        => await NavigateToEdit.InvokeAsync(termId);

    protected async Task OnIsNotInDbChanged(Guid termId)
        => await IsNotInDbChanged.InvokeAsync(termId);

    protected async Task OnSelectedCategoriesChanged(Guid termId, IEnumerable<Category> categories)
        => await SelectedCategoriesChanged.InvokeAsync((termId, categories));
}