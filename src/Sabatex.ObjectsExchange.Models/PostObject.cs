namespace Sabatex.ObjectsExchange.Models
{
#if NET6_0_OR_GREATER
#nullable enable
    using Sabatex.Core;
    using System.ComponentModel.DataAnnotations;
    using System;
    public class PostObject : IEntityBase
    {
        [MaxLength(50)]
        public string objectType = string.Empty;
        [MaxLength(50)]
        public string objectId = string.Empty;
        public string text = string.Empty;
        public DateTime dateStamp = DateTime.Now;
        string IEntityBase.KeyAsString() => objectId.ToString();
    }
#endif
}