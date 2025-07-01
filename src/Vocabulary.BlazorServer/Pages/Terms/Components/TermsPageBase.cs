using MudBlazor;
using MudBlazor.Services;

namespace Vocabulary.BlazorServer.Pages.Terms.Components;

public class TermsPageBase : ComponentBase
{
    [Inject]
    private IBrowserViewportService ResizeService { get; set; } = default!;

    private const int DRAWER_WIDTH = 240;
    private Guid _subscriptionId = Guid.NewGuid();

    protected BreakPt Breakpoints = BreakPt.Lg;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ResizeService.SubscribeAsync(
                _subscriptionId, 
                (size) =>
                {
                    if (TrySetBreakpoints(size.BrowserWindowSize))
                    {
                        InvokeAsync(StateHasChanged);
                    }
                }, 
                new ResizeOptions
                {
                    ReportRate = 50,
                    NotifyOnBreakpointOnly = false,
                });

            BrowserWindowSize size = await ResizeService.GetCurrentBrowserWindowSizeAsync();
            if (TrySetBreakpoints(size))
            {
                StateHasChanged();
            }
        }

        //if (_termList.CategoryName != CategoryName ) {
        //    _termList!.CategoryName = CategoryName;
        //}

        await base.OnAfterRenderAsync(firstRender);
    }

    private bool TrySetBreakpoints(BrowserWindowSize size)
    {
        if (size.Width > 1580 + DRAWER_WIDTH)
        {
            if (Breakpoints != BreakPt.Lg)
            {
                Breakpoints = BreakPt.Lg;
                return true;
            }
        }
        else if (size.Width > 1401 + DRAWER_WIDTH)
        {
            if (Breakpoints != BreakPt.Md)
            {
                Breakpoints = BreakPt.Md;
                return true;
            }
        }
        else if (size.Width > 920)
        {
            if (Breakpoints != BreakPt.Sm)
            {
                Breakpoints = BreakPt.Sm;
                return true;
            }
        }
        else
        {
            if (Breakpoints != BreakPt.Xs)
            {
                Breakpoints = BreakPt.Xs;
                return true;
            }
        }

        return false;
    }

    public async ValueTask DisposeAsync() => await ResizeService.UnsubscribeAsync(_subscriptionId);

    protected enum BreakPt
    {
        Lg,
        Md,
        Sm,
        Xs
    }
}