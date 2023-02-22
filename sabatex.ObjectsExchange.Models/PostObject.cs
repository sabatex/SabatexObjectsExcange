using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sabatex.ObjectsExchange.Models;

public class PostObject
{
    /// <summary>
    /// The list id's destination nodes with separated char ,
    /// </summary>
    public Guid Destination { get; set; }
    public string ObjectType { get; set; } = default!;
    public string ObjectId { get; set; } = default!;
    public DateTime DateStamp { get; set; }
    public string ObjectAsText { get; set; } = default!;
    public PostObject(Guid destination, string objectType, string objectId, DateTime dateStamp, string objectAsText)
    {
        Destination = destination;
        ObjectType = objectType;
        ObjectId = objectId;
        DateStamp = dateStamp;
        ObjectAsText = objectAsText;
    }
}
