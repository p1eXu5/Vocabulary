using System.ComponentModel;
using System.Runtime.CompilerServices;
using MudBlazor;

namespace Vocabulary.BlazorServer;

public class VocabularyMudTheme : MudTheme, INotifyPropertyChanged
{
    internal const int DRAWER_WIDTH_LEFT = 240;

    private int _drawerWidthLeft = DRAWER_WIDTH_LEFT; // Default value for units

    public VocabularyMudTheme() : base()
    {
        // PaletteDark.Primary = new MudBlazor.Utilities.MudColor("ebd2b7");
        PaletteDark.Secondary = new MudBlazor.Utilities.MudColor("6be77f");

        LayoutProperties.DrawerWidthLeft = DRAWER_WIDTH_LEFT + "px";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int? DrawerWidthLeft
    {
        get => _drawerWidthLeft;
        set
        {
            if (_drawerWidthLeft != value)
            {
                _drawerWidthLeft = value ?? DRAWER_WIDTH_LEFT;
                LayoutProperties.DrawerWidthLeft = $"{_drawerWidthLeft}px";
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = default)
        => PropertyChanged?.Invoke(this, new(propertyName));

    public void SetDrawerWidthLeft(int? width)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        DrawerWidthLeft = width;
    }
}