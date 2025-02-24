using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.BASAdapter;

public record ObjectHeader(string type,string id);
public record ObjectQueryHeader(ObjectHeader query);
