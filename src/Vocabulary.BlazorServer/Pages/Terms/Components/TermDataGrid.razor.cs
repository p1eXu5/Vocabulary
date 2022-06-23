using Fluxor;
using Fluxor.Blazor.Web.Components;
using MediatR;
using Quartz.Util;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.DataContracts.Types;
using Vocabulary.Descriptions;
using Vocabulary.WebClient.Store;
using Vocabulary.WebClient.Store.Types;
using Vocabulary.WebClient.Store.VocabularyState;

#nullable enable

namespace Vocabulary.BlazorServer.Pages.Terms.Components;

using TermListMsg = Vocabulary.WebClient.Store.TermListState.Msg;

public partial class TermDataGrid : FluxorComponent
{
    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public string? CategoryName { get; set; }

    [Parameter]
    public string? Search { get; set; }


    [Inject]
    private IDbContextFactory<VocabularyDbContext> DbContextFactory { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AppState AppState { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<VocabularyState> VocabularyState { get; set; } = default!;
    private TermListState TermListState => VocabularyState.Value.TermListState;

    private bool IsLoading => TermListState.IsTermsLoading;

    private IEnumerable<FullTerm> Terms => TermListState.GetTerms();

    private string? _searchString;
    private string? _categoryName;

    private bool IsFilteringByCategory => !string.IsNullOrWhiteSpace(CategoryName);
    private string Title => IsFilteringByCategory ? $"{CategoryName} Terms" : "All Terms";


    // quick filter - filter globally across multiple columns with the same input
    private Func<FullTerm, bool> _quickFilter => x =>
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        var searchItems = _searchString?.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

        if (searchItems?.Any() != true)
        {
            return true;
        }

        if (AppState.SearchInTerms && searchItems.Any(it => x.Name.Contains(it, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (AppState.SearchInTerms && searchItems.Any(it => x.AdditionalName?.Contains(it, StringComparison.OrdinalIgnoreCase) == true))
        {
            return true;
        }

        if (AppState.SearchInSynonyms && searchItems.Any(it => x.Synonyms.Any(s => s.Name.Contains(it, StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        if (AppState.SearchInCategories && searchItems.Any(it => x.Categories.Any(s => s.Name.Contains(it, StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        return false;
    };

    protected override void OnParametersSet()
    {
        _categoryName = CategoryName;
        _searchString = Search;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(Msg.NewTermListMsg(TermListMsg.LoadTerms));
    }

    private void ReloadTermsForSearch(string searchString)
    {
        if (searchString.Equals(_searchString))
        {
            return;
        }

        _searchString = searchString;


        if (_categoryName.IsNullOrWhiteSpace())
        {
            return;
        }

        if (!AppState.SearchInCurrentCategory)
        {
            _categoryName = null;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameters("/terms", new Dictionary<string, object?>()
            {
                ["Search"] = _searchString,
                ["CategoryName"] = _categoryName
            }));
        }
    }

    // events
    private void NavigateToCreate()
    {
        NavigationManager.NavigateTo(
            NavigationManager.GetUriWithQueryParameters("/term/create",
                new Dictionary<string, object?>
                {
                    ["returnUrl"] = NavigationManager.Uri
                }
            ));
    }

    internal void NavigateToEdit(string id)
    {
        NavigationManager.NavigateTo(
            NavigationManager.GetUriWithQueryParameters("/term/" + id,
                new Dictionary<string, object?>
                {
                    ["returnUrl"] = NavigationManager.Uri
                }
            ));
    }

    internal void NavigateToCopy(string id)
    {
        NavigationManager.NavigateTo(
            NavigationManager.GetUriWithQueryParameters("/term/create",
                new Dictionary<string, object?>
                {
                    ["idToCopy"] = id,
                    ["returnUrl"] = NavigationManager.Uri
                }
            ));
    }

    internal void FindLinksInDescription(Guid termId)
        => Dispatcher.Dispatch(Msg.NewTermListMsg(TermListMsg.FindLinksInDescription(termId)));


    internal async Task RemoveTerm(Guid termId)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var term = await dbContext.Terms.Include(t => t.TermCategories).SingleOrDefaultAsync(t => t.Id == termId);

        if (term is not null)
        {
            foreach (var termCategory in term.TermCategories.ToList())
            {
                term.TermCategories.Remove(termCategory);
            }

            dbContext.Terms.Remove(term);
            await dbContext.SaveChangesAsync();
        }

        // TODO: change with action - Terms.Remove(Terms.Single(t => t.Id == termId));

        // await InvokeAsync(StateHasChanged);
        await AppState.NotifyCategoryChangedAsync(new AsyncEventArgs() { RemovedTermId = termId });
    }
}
