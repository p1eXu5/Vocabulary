
namespace Vocabulary.BlazorServer;

public class AsyncEventArgs
{
    public Guid? NewCategoryId { get; set; }
    public Guid? RemovedCategoryId { get; set; }
    public Guid? RemovedTermId { get; set; }
}

public delegate Task AsyncEventHandler(AsyncEventArgs args);

public class AppState
{
    public bool SearchInTerms { get; set; } = true;
    public bool SearchInCategories { get; set; } = true;
    public bool SearchInSynonyms { get; set; } = true;

    internal event AsyncEventHandler? OnCategoriesChangedAsync;

    internal async Task NotifyCategoryChangedAsync(AsyncEventArgs args)
    {
        if (OnCategoriesChangedAsync is not null) {
            await OnCategoriesChangedAsync.Invoke(args);
        }
    }
}
