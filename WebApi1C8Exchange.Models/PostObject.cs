using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

/// <summary>
/// дані для передачі JSON
/// </summary>
/// <param name="DestinationNode">отримувач</param>
/// <param name="Id">унікальне id відправлення</param>
/// <param name="DateStamp">часова позначка відправлення</param>
/// <param name="ObjectType">тип обєкта</param>
/// <param name="ObjectId">унікальне id обэкта</param>
/// <param name="ObjectJson">сам object</param>
public record struct PostObject(string DestinationNode,
                                Guid Id, 
                                DateTime DateStamp,
                                string ObjectType,
                                Guid ObjectId,
                                string ObjectJson);
