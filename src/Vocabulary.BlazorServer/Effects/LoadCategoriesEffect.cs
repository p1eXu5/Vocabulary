using Fluxor;
using Vocabulary.Categories.Ports;

namespace Vocabulary.BlazorServer.Effects;




public class LoadCategoriesEffect
{
    private readonly ICategoryRepository _categoryRepository;

    public LoadCategoriesEffect(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    //public async Task LoadCategoriesAsync(VocabularyStateModule.Msg msg, IDispatcher dispatcher)
    //{ }
}
