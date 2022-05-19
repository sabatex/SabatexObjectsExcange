using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

/// <summary>
/// Список обєктів для нода
/// </summary>
/// <param name="node">назва нода</param>
/// <param name="apiKey">ключ для нода</param>
/// <param name="ObjectsId">ід обєкта</param>
public record struct ExchangeObjectsId(string[] ObjectsId);