namespace Sabatex.ObjectsExchange.Models
{
    using Sabatex.Core;
    using System.Collections.Generic;
    /// <summary>
    /// Helper class for part result collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultCollection<T> where T : IEntityBase
    {
        /// <summary>
        /// Part items collection
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new T[] { };
        /// <summary>
        /// total items count in database 
        /// </summary>
            public int Count { get; set; }
        }
}
