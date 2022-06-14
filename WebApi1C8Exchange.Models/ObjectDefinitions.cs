using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;
public record  QueryedObject(string Destination,string ObjectType, string ObjectId);
/// <summary>
/// 
/// </summary>
/// <param name="Destination">sumbolic name or digital id</param>
/// <param name="ObjectId"></param>
/// <param name="ObjectType">sumbolic name or digital id</param>
/// <param name="ObjectJSON"></param>
public record PostObject(string Destination, string ObjectType, string ObjectId,DateTime DateStamp, string ObjectJSON):QueryedObject(Destination,ObjectType,ObjectId);

