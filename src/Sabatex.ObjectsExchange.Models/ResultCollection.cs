namespace Sabatex.ObjectsExchange.Models
{
    using Sabatex.Core;
    using System.Collections.Generic;
    public class ResultCollection<T> where T : IEntityBase
    {
            public IEnumerable<T> Items { get; set; } = new T[] { };
            public int Count { get; set; }
        }
}
