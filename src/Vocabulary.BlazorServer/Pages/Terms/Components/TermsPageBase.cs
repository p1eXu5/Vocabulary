using MudBlazor.Services;

namespace Vocabulary.BlazorServer.Pages.Terms.Components;

public class TermsPageBase : ComponentBase
{
    [Inject]
    private IResizeService ResizeService { get; set; } = default!;

    private const int DRAWER_WIDTH = 240;
    private Guid _subscriptionId;

    protected Breakponts Breakpoints = Breakponts.Lg;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _subscriptionId = await ResizeService.Subscribe((size) =>
            {
                if (TrySetBreakpoints(size))
                {
                    InvokeAsync(StateHasChanged);
                }
            }, new ResizeOptions
            {
                ReportRate = 50,
                NotifyOnBreakpointOnly = false,
            });

            BrowserWindowSize size = await ResizeService.GetBrowserWindowSize();
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
            if (Breakpoints != Breakponts.Lg)
            {
                Breakpoints = Breakponts.Lg;
                return true;
            }
        }
        else if (size.Width > 1401 + DRAWER_WIDTH)
        {
            if (Breakpoints != Breakponts.Md)
            {
                Breakpoints = Breakponts.Md;
                return true;
            }
        }
        else if (size.Width > 920)
        {
            if (Breakpoints != Breakponts.Sm)
            {
                Breakpoints = Breakponts.Sm;
                return true;
            }
        }
        else
        {
            if (Breakpoints != Breakponts.Xs)
            {
                Breakpoints = Breakponts.Xs;
                return true;
            }
        }

        return false;
    }

    public async ValueTask DisposeAsync() => await ResizeService.Unsubscribe(_subscriptionId);

    protected enum Breakponts
    {
        Lg,
        Md,
        Sm,
        Xs
    }
}