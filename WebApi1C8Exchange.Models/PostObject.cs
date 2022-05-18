using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

/// <summary>
/// дані для передачі JSON
/// </summary>
/// <param name="SenderNode">відправник</param>
/// <param name="DestinationNode">отримувачі</param>
/// <param name="ApiKey">ключ від апі</param>
/// <param name="ObjectType">тип обєкта</param>
/// <param name="ObjectId">унікальне id обэкта</param>
/// <param name="ObjectJson">сам object</param>
public record struct PostObject(string[] DestinationNode,
                                string ObjectType,
                                string ObjectId,
                                string ObjectJson);
