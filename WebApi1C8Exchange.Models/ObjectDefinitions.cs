using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

public record ObjectDescriptor(string ObjectId,string ObjectTypeName);
public record ObjectDescriptorWithBody(string ObjectId, string ObjectTypeName,string ObjectJSON);
public record ShortObjectDescriptorWithBody(string ObjectId, int ObjectTypeId, string ObjectJSON);
public record struct QueryedObject(string Destination, string ObjectId, string ObjectTypeName);

