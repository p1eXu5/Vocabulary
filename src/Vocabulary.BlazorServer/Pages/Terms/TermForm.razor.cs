using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;
using Vocabulary.Adapters.Persistance;
using Vocabulary.Adapters.Persistance.Models;
using Vocabulary.BlazorServer.Pages.Terms.Components;
using Vocabulary.Categories.DataContracts;
using Vocabulary.Terms;
using Vocabulary.Terms.Abstractions;
using Vocabulary.Terms.DataContracts;
using DbCategory = Vocabulary.Adapters.Persistance.Models.Category;
using DbSynonym = Vocabulary.Adapters.Persistance.Models.Synonym;
using Link = Vocabulary.Adapters.Persistance.Models.Link;
using Synonym = Vocabulary.Terms.DataContracts.Synonym;

namespace Vocabulary.BlazorServer.Pages.Terms;

public partial class TermForm : IAsyncDisposable
{
    private readonly MudTheme _mubTheme = new MudTheme();

    [Inject] public IDbContextFactory<VocabularyDbContext> DbContextFactory { get; private set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; private set; } = default!;
    [Inject] public IDialogService DialogService { get; private set; } = default!;
    [Inject] public AppState AppState { get; private set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private TermNamesComparer TermNamesComparer { get; } = new();

    [Parameter]
    public string? TermId { private get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? IdToCopy { private get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { private get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? CategoryId { private get; set; }

    private string? _action;
    private bool _addSearchLink;
    private bool _dialogIsShown;

    private VocabularyDbContext _dbContext = default!;
    private List<DbCategory> _categories = new List<DbCategory>();

    private Term _term = new();
    private Adapters.Persistance.Models.Link _newLink = new();
    private DbSynonym _newSynonym = new();


    private int Lines { get; set; } = 8;
    private string _value { get; set; } = "Nothing selected";

    private bool _descriptionExpanded = true;
    private bool _categoryExpanded = true;
    private bool _termExpanded = true;

    private List<TermNames> _allTermNames = default!;
    private List<ITermNames> _similarTermNames = new List<ITermNames>(2);


    protected override Task OnParametersSetAsync()
    {
        _action =
            string.IsNullOrWhiteSpace(TermId)
                ? "Create"
                : "Update";

        return LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_dbContext is null)
        {
            _dbContext = await DbContextFactory.CreateDbContextAsync();
        }

        _categories = await _dbContext.Categories.ToListAsync();
        _allTermNames = (await _dbContext.Terms.Include(t => t.Synonyms).ToListAsync()).Select(Mapper.Map<TermNames>).ToList();

        if (!string.IsNullOrWhiteSpace(TermId) && Guid.TryParse(TermId, out var termId)) {

            var dbTerm = await _dbContext.Terms.Include(t => t.Links).Include(t => t.Synonyms).Include(t => t.Categories).Where(t => t.Id == termId).SingleOrDefaultAsync();
            if (dbTerm is not null) {
                _term = dbTerm;
            }
        }
        else if (!string.IsNullOrWhiteSpace(IdToCopy) && Guid.TryParse(IdToCopy, out var idToCoppy)) {

            var dbTerm = await _dbContext.Terms.Include(t => t.Links).Include(t => t.Synonyms).Include(t => t.Categories).Where(t => t.Id == idToCoppy).SingleOrDefaultAsync();
            if (dbTerm is not null) {
                foreach (var link in dbTerm.Links) {
                    _term.Links.Add(link.CloneToNewLink());
                }

                foreach (var category in dbTerm.Categories) {
                    _term.Categories.Add(_categories.First(c => c.Id == category.Id));
                }
            }
        }

        if (
                CategoryId.HasValue 
                && !_term.Categories.Any(c => c.Id == CategoryId.Value)
                && _categories.Any(c => c.Id == CategoryId.Value)
        ) {
            _term.Categories.Add(_categories.First(c => c.Id == CategoryId.Value));
        }
    }



    private void OnSelectedCategoriesChanged(IEnumerable<DbCategory> selectedValues)
    {
        _term.Categories = selectedValues.ToHashSet();
    }

    private void OnValidSynonymSubmit()
    {
        _term.Synonyms.Add(_newSynonym);
        _newSynonym = new();
    }

    private void OnValidLinkSubmit()
    {
        if (_addSearchLink) {
            _newLink.ResourceDescription = _newLink.Href;
            _newLink.Href = "/?search=" + WebUtility.UrlEncode(_newLink.Href);
        }

        _term.Links.Add(_newLink);
        _newLink = new();
    }

    private void DeleteLink(int linkId)
    {
        Link? link = _term.Links.FirstOrDefault(c => c.Id == linkId);

        if (link is not null)
        {
            _term.Links.Remove(link); 
        }
    }

    private async Task CreateCategory()
    {
        var result = await ShowCategoryDialog();

        if (!result.Cancelled && Guid.TryParse(result.Data.ToString(), out Guid categoryId)) {
            var newCategory = await _dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
            if (newCategory is not null) {
                _term.Categories.Add(newCategory);
                _categories.Add(newCategory);
            }
        }
    }

    private async Task EditCategory(Guid categoryId)
    {
        var parameters = new DialogParameters { ["CategoryId"] = categoryId };

        _dialogIsShown = true; // prevent receive focus on multi select drop down (Blazor bug)
        var result = await ShowCategoryDialog(parameters);
        _dialogIsShown = false;

        if (!result.Cancelled) {
            var termCategoru = _term.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (termCategoru is not null) {
                _term.Categories.Remove(termCategoru);
            }

            var category = await _dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
            if (category is not null) {
                await _dbContext.Entry(category).ReloadAsync();
            }

            StateHasChanged();
        }
    }

    private async Task DeleteCategory(Guid categoryId)
    {
        var category = await _dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
        if (category is not null) {
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            _categories.Remove(_categories.Single(c => c.Id == category.Id));

            if (_term.Categories.Any(c => c.Id == category.Id)) {
                _term.Categories.Remove(_term.Categories.Single(c => c.Id == category.Id));
            }

            await AppState.NotifyCategoryChangedAsync(new AsyncEventArgs() { RemovedCategoryId = categoryId});
        }
    }

    private Task<DialogResult> ShowCategoryDialog(DialogParameters? parameters = null)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
        if (parameters is null) {

        }
        var dialog =
            parameters is null
                ? DialogService.Show<CategoryForm>("Create Category", options)
                : DialogService.Show<CategoryForm>("Edit Category", parameters, options);

        return dialog.Result;
    }

    private async Task OnValidTermSubmit()
    {
        if (_newLink.Href.NotNullOrWhiteSpace())
        {
            OnValidLinkSubmit();
        }

        if (_newSynonym.Name.NotNullOrWhiteSpace())
        {
            OnValidSynonymSubmit();
        }

        AsyncEventArgs asyncEventAgs = new();

        if (!string.IsNullOrWhiteSpace(TermId) && Guid.TryParse(TermId, out var termId)) {

            _dbContext.Terms.Update(_term);
        }
        else {
            var seq = await _dbContext.Terms.OrderBy(t => t.Sequence).LastAsync();
            _term.Sequence = seq.Sequence + 1;
            _dbContext.Add(_term);

            asyncEventAgs.NewCategoryId = _term.Id;
        }

        await _dbContext.SaveChangesAsync();
        await AppState.NotifyCategoryChangedAsync(asyncEventAgs);

        if (!string.IsNullOrWhiteSpace(ReturnUrl)) {
            NavigationManager.NavigateTo(ReturnUrl);
        }
        else {
            NavigationManager.NavigateTo("/");
        }
    }

    private void CheckSimilarTerms()
    {
        ITermNames tn = new TermNames(_term.Id, _term.Sequence, _term.Name, _term.AdditionalName, _term.Synonyms.Select(s => new Synonym(s.Name)).ToArray());
        _similarTermNames = TermNamesComparer.Compare(tn, _allTermNames, Vocabulary.Terms.Enums.ComparingNames.MainOrAdditional).ToList();
    }

    private async Task NavigateToExistingTerm(string termName) // TODO: replace with id
    {
        var url = NavigationManager.GetUriWithQueryParameters("/terms", new Dictionary<string, object?> { ["Search"] = termName});
        await JS.InvokeAsync<object>("open", url, "_blank");
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext is not null) {
            await _dbContext.DisposeAsync();
        }
    }
}
