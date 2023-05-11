namespace sabatex.ObjectsExchange.Models
{
#if NET3_5 || NETSTANDARD2_0
    using Sabatex.Core;
    using System;
    /// <summary>
    /// обєкти які потрібно отримати з нода
    /// </summary>
    public class QueryObject : IEntityBase
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        public string ObjectId { get; set; } = string.Empty; //must lovercase
        public string ObjectType { get; set; } = string.Empty; // lovercase
        string IEntityBase.KeyAsString() => Id.ToString();
    }
#endif
}