#if NET6_0_OR_GREATER
using Sabatex.Core;
using System.ComponentModel.DataAnnotations;
#endif

namespace sabatex.ObjectsExchange.Models
{
    /// <summary>
    /// обєкти які потрібно отримати з нода
    /// </summary>
#if NET6_0_OR_GREATER
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