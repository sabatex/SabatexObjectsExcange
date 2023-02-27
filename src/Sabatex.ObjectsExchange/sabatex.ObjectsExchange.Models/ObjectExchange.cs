#if NET6_0_OR_GREATER
using Sabatex.Core;
using System.ComponentModel.DataAnnotations;
#endif

namespace sabatex.ObjectsExchange.Models
{
#if NET6_0_OR_GREATER
    public class ObjectExchange : IEntityBase
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        [Required]
        [MaxLength(50)]
        public string ObjectId { get; set; } = default!;
        [MaxLength(50)]
        public string ObjectType { get; set; } = default!;
        /// <summary>
        /// внутрішня позначка часу створення обєкта
        /// </summary>
        public DateTime DateStamp { get; set; } = DateTime.Now;
        public string ObjectAsText { get; set; } = default!;

        string IEntityBase.KeyAsString() => Id.ToString();

    }
#else
    using System;
    public class ObjectExchange
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        public string ObjectId { get; set; }
        public string ObjectType { get; set; }
        /// <summary>
        /// внутрішня позначка часу створення обєкта
        /// </summary>
        public DateTime DateStamp { get; set; }
        public string ObjectAsText { get; set; }
    }
#endif
}
