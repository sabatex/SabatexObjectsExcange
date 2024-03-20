using Microsoft.AspNetCore.Authorization;
using Sabatex.Core;
using Sabatex.RadzenBlazor;

namespace ObjectsExchange.Client.Pages;

[Authorize]
public abstract class GridBasePage<TItem> : SabatexRadzenBlazorBaseGridPage<TItem, Guid> where TItem : class, IEntityBase<Guid>
{
}
