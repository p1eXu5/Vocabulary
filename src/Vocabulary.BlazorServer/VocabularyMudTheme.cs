using MudBlazor;

namespace Vocabulary.BlazorServer;

public class VocabularyMudTheme : MudTheme
{
    public VocabularyMudTheme() : base()
    {
        // PaletteDark.Primary = new MudBlazor.Utilities.MudColor("ebd2b7");
        PaletteDark.Secondary = new MudBlazor.Utilities.MudColor("6be77f");
    }
}