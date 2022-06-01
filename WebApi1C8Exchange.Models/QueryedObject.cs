using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;


public record struct QueryedObject(Guid ownerId, Guid objectId, string ObjectType);

