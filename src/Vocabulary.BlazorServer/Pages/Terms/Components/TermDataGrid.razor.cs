using Fluxor;
using Fluxor.Blazor.Web.Components;
using MediatR;
using Quartz.Util;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.Descriptions;
using Vocabulary.WebClient.Store;

#nullable enable

namespace Vocabulary.BlazorServer.Pages.Terms.Components;

public partial class TermDataGrid : FluxorComponent
{
    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Inject]
    private IDbContextFactory<VocabularyDbContext> DbContextFactory { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AppState AppState { get; set; } = default!;

    [Inject]
    private IMediator Mediator { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<VocabularyState> State { get; set; } = default!;

    [Parameter]
    public string? CategoryName { get; set; }

    [Parameter]
    public string? Search { get; set; }


    private List<Term> _terms = new List<Term>();
    private string? _searchString;
    private string? _categoryName;
    private bool _isLoading = true;

    private bool IsFilteringByCategory => !string.IsNullOrWhiteSpace(CategoryName);
    private string Title => IsFilteringByCategory ? $"{CategoryName} Terms" : "All Terms";


    // quick filter - filter globally across multiple columns with the same input
    private Func<Term, bool> _quickFilter => x =>
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

    protected override async Task OnParametersSetAsync()
    {
        _categoryName = CategoryName;
        _searchString = Search;
        await LoadDataAsync();
    }


    private async Task LoadDataAsync()
    {
        _isLoading = true;
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var query = dbContext.Terms.Include(t => t.Synonyms).Include(t => t.Links).Include(t => t.Categories).OrderBy(t => t.Sequence);

        if (!string.IsNullOrWhiteSpace(_categoryName))
        {
            _terms =
                _categoryName.Equals("Uncategorized", StringComparison.Ordinal)
                    ? await query.Where(t => !t.Categories.Any()).ToListAsync()
                    : await query.Where(t => t.Categories.Any(c => c.Name == CategoryName)).ToListAsync();
        }
        else
        {
            _terms = await query.ToListAsync();
        }

        _searchString = Search;
        _isLoading = false;
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

    internal async Task CheckDecriptionTerms(Guid termId)
    {
        var res = await Mediator.Send(new CheckTermsCommand(termId));

        if (res.TryGetSucceededContext(out var newDescription))
        {
            var term = _terms.SingleOrDefault(t => t.Id == termId);
            if (term is not null)
            {
                term.Description = newDescription;
            }
        }
    }

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

        _terms.Remove(_terms.Single(t => t.Id == termId));

        // await InvokeAsync(StateHasChanged);
        await AppState.NotifyCategoryChangedAsync(new AsyncEventArgs() { RemovedTermId = termId });
    }
}
