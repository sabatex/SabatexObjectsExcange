namespace Sabatex.ObjectsExchange.Models;
#if NET3_5 || NETSTANDARD2_0
using Sabatex.Core;
public class PostObject : IEntityBase {
    public string objectType = string.Empty;
    public string objectId = string.Empty;
    public string text = string.Empty;
    public DateTime dateStamp = DateTime.Now;
    string IEntityBase.KeyAsString() => objectId.ToString();
}
#endif
