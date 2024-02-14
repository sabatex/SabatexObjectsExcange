using Sabatex.Core;
using System.ComponentModel.DataAnnotations;
using System;
namespace Sabatex.ObjectsExchange.Models
{
        /// <summary>
        /// обєкти які потрібно отримати з нода
        /// </summary>
        public class QueryObject : IEntityBase<long>
        {
        /// <summary>
        /// primary key
        /// </summary>
            public long Id { get; set; }
        /// <summary>
        /// Sender client id
        /// </summary>
            public Guid Sender { get; set; }
        /// <summary>
        /// Destination client id
        /// </summary>
            public Guid Destination { get; set; }
        /// <summary>
        /// Client object unique id
        /// </summary>
        [MaxLength(50)]
        public string ObjectId { get; set; } = default!; //must lovercase
        /// <summary>
        /// client object type
        /// </summary>
        [MaxLength(50)]
        public string ObjectType { get; set; } = default!; // lovercase
            //string IEntityBase.KeyAsString() => Id.ToString();
        }

}