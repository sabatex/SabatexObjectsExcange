using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.BASAdapter;
public static class BAS
{
    public static string GetObjectHeader(string type, string id)
    {
        return $"{{\"type\":\"{type}\",\"id\":\"{id}\"}}";
    }
}

public record ObjectHeader(string type,string id);
public record ObjectQueryHeader(ObjectHeader query);
