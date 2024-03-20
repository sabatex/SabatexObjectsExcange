using Sabatex.Core;
using Sabatex.RadzenBlazor;

namespace ObjectsExchange.Client.Pages;

public abstract class EditBasePage<TItem>:SabatexRadzenBlazorBaseEditPage<TItem,Guid> where TItem : class,IEntityBase<Guid>, new()
{
}
