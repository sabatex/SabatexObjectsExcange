using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Sabatex.Core;
using Sabatex.RadzenBlazor;

namespace Sabatex.ObjectsExchange.Client.Shared;

public abstract class BaseDataGridPage<TItem> : ComponentBase where TItem : class,IEntityBase
{
    [Inject] protected IRadzenDataAdapter DataAdapter { get; set; }=default!;
    [Inject] protected DialogService? DialogService { get; set; }
    [Inject] protected NotificationService? NotificationService { get; set; }

    protected bool isGridDataLoading = false;
    protected RadzenDataGrid<TItem>? grid;
    protected virtual string? expandGrid => null;
    protected RadzenODataCollection<TItem> dataCollection = new RadzenODataCollection<TItem>();
    private void GridRefNull()
    {
        NotificationService?.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = "The grid reference not initialize!"
        });
    }
    private async Task GridReload()
    {
        if (grid != null)
        {
            await grid.Reload();
        }
        else
        {
            GridRefNull();
        }
    }
    protected async Task GridDeleteButtonClick(TItem data)
    {
        if (DialogService == null) return;
        try
        {
            if (await DialogService.Confirm("Ви впевнені?", "Видалення запису", new ConfirmOptions() { OkButtonText = "Так", CancelButtonText = "Ні" }) == true)
            {
                await DataAdapter.DeleteAsync<TItem>(data.KeyAsString());
                await GridReload();
            }
        }
        catch (System.Exception e)
        {
            string errorMessage = $"Не можливо видалити Error:{e.Message}";
            Console.WriteLine(errorMessage);
            if (NotificationService != null)
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Помилка",
                    Detail = $"Не можливо видалити Error:{e.Message}"
                });
            }

        }
    }

}
