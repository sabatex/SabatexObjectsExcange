namespace sabatex.ObjectsExchange.Models
{
#if NET3_5
    using Sabatex.Core;
    using System.ComponentModel.DataAnnotations;

    public class ObjectExchange : IEntityBase
    {
        public long Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Destination { get; set; }
        public string ObjectId { get; set; }=string.Empty;
        public string ObjectType { get; set; }=string.Empty;
        /// <summary>
        /// внутрішня позначка часу створення обєкта
        /// </summary>
        public DateTime DateStamp { get; set; } = DateTime.Now;
        public DateTime SenderDateStamp { get; set; }
        public string ObjectAsText { get; set; } = default!;
        string IEntityBase.KeyAsString() => Id.ToString();
    }
#endif
}
