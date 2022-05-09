using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

public enum ExchangeStatus:byte
{
    // обьєкт тільки відправлений
    New,
    // обьєкт частково оброблений і чекає на додаткові дані э запит в ObjectQuery
    Waited,
    // обьєкт переданий успішно
    Resived,
    Done
}
