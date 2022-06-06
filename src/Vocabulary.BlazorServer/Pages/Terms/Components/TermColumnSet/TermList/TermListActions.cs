namespace Vocabulary.BlazorServer.Pages.Terms.Components.TermColumnSet.TermList;

public abstract class TermListActions : ComponentBase
{
    [Parameter]
    public EventCallback<string> NavigateToEdit { get; set; }

    [Parameter]
    public EventCallback<string> NavigateToCopy { get; set; }

    [Parameter]
    public EventCallback<Guid> CheckDecriptionTerms { get; set; }

    [Parameter]
    public EventCallback<Guid> RemoveTerm { get; set; }

    [Parameter]
    public string BroadestColumnMinWidth { get; set; } = "";




    protected async Task OnNavigateToEdit(string termId)
        => await NavigateToEdit.InvokeAsync(termId);

    protected async Task OnNavigateToCopy(string termId)
        => await NavigateToCopy.InvokeAsync(termId);

    protected async Task OnCheckDecriptionTerms(Guid termId)
        => await CheckDecriptionTerms.InvokeAsync(termId);

    protected async Task OnRemoveTerm(Guid termId)
        => await RemoveTerm.InvokeAsync(termId);
}