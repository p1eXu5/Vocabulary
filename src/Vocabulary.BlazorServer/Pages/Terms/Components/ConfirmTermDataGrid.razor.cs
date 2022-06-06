using Microsoft.Extensions.Caching.Memory;
using MudBlazor;
using p1eXu5.Result;
using System.Collections.Immutable;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Terms.DataContracts;
using Vocabulary.Terms.Ports;

namespace Vocabulary.BlazorServer.Pages.Terms.Components;

public partial class ConfirmTermDataGrid
{
    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter, EditorRequired]
    public string ConfirmImportingTermsKey { private get; set; } = default!;

    [Inject]
    private IMemoryCache MemoryCache { get; init;} = default!;

    [Inject]
    private ITermRepository TermRepository { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<ConfirmTermDataGrid> Logger { get; set; } = default!;

    private bool _isLoading = true;
    private string? _searchString;

    private ImmutableArray< ConfirmImportingTerm > _confirmImportingTerms = new ImmutableArray< ConfirmImportingTerm >();

    private bool _searchInNames = true;
    private bool _searchInAdditionsNames = true;
    private bool _searchInSynonyms = true;

    private static string Title => "Importing Terms";


    // quick filter - filter globally across multiple columns with the same input
    private Func<ConfirmImportingTerm, bool> _quickFilter => x =>
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

        if (_searchInNames && searchItems.Any(it => x.ImportingTerm.Name.Contains(it, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (_searchInAdditionsNames && searchItems.Any(it => x.ImportingTerm.AdditionalName?.Contains(it, StringComparison.OrdinalIgnoreCase) == true))
        {
            return true;
        }

        if (_searchInSynonyms && searchItems.Any(it => x.ImportingTerm.Synonyms.Any(s => s.Name.Contains(it, StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        return false;
    };


    protected override void OnInitialized()
    {
        _isLoading = true;
        _confirmImportingTerms = MemoryCache.Get<ImmutableArray<ConfirmImportingTerm>>(ConfirmImportingTermsKey);
        _isLoading = false;
    }

    internal void ToggleIsNotInDb(Guid importingTermId)
    {
        var importingTerm = _confirmImportingTerms.Single(t => t.ImportingTerm.Id == importingTermId);
        importingTerm.IsNotInDb = !importingTerm.IsNotInDb;
    }

    internal void SetCategories(Guid importingTermId, IEnumerable<Category> categories)
    {
        var importingTerm = _confirmImportingTerms.Single(t => t.ImportingTerm.Id == importingTermId);
        importingTerm.Categories = categories is ImmutableArray<Category> imm ? imm : categories.ToImmutableArray();
    }

    private async Task StoreSelectedTerms()
    {
        Result result = await TermRepository.ImportAsync(_confirmImportingTerms.Where(t => t.IsNotInDb).ToArray());
        
        if (result)
        {
            NavigationManager.NavigateTo("/", true);
            return;
        }

        Logger.LogError("{errorMessage}", result.ToString()); 

        // TODO: show alert
        Snackbar.Add(result.ToString(), Severity.Error, config => { 
            config.VisibleStateDuration = 100_000;
        });
    }
}