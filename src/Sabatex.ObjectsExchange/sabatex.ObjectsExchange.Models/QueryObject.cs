namespace sabatex.ObjectsExchange.Models
{
#if NET6_0_OR_GREATER
using Sabatex.Core;
using System.ComponentModel.DataAnnotations;
    /// <summary>
    /// обєкти які потрібно отримати з нода
    /// </summary>
    public class QueryObject : IEntityBase
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        [MaxLength(50)]
        public string ObjectId { get; set; } = default!; //must lovercase
        [MaxLength(50)]
        public string ObjectType { get; set; } = default!; // lovercase

        string IEntityBase.KeyAsString() => Id.ToString();

    }
#else
    using System;
       /// <summary>
    /// обєкти які потрібно отримати з нода
    /// </summary>
    public class QueryObject
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        public string ObjectId { get; set; }
        public string ObjectType { get; set; }
    }

#endif
}