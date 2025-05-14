using Microsoft.Extensions.Localization;
using Sabatex.ObjectExchange.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.BASAdapter
{
    public class MessageAnalizer : IExchangeObjectAnalizer
    {
        private bool success = true;
        private string objectId = string.Empty;
        private string objectType = string.Empty;
        private readonly IStringLocalizer _loc;
        public MessageAnalizer(IStringLocalizer stringLocalizer)
        {
            _loc = stringLocalizer;
        }

        bool IsQuery(JsonDocument jsonDocument)
        {
            if (jsonDocument.RootElement.TryGetProperty("query", out var query))
            {
                if (query.ValueKind == JsonValueKind.Object)
                {
                    return true;
                }
            }
            return false;
        }


        public async Task<bool> QueryAnalize(AnalizerObjectContextBase context, JsonValueWrapper query)
        {
            return context.Error(_loc["Даний запит ще не реалізовано"]);
        }


        public async Task<bool> MessageAnalize(AnalizerObjectContextBase context, string messageHeader, string? message)
        {
            JsonValueWrapper header;
            try
            {
                header = JsonParser.ParseJson(messageHeader);
            }
            catch (Exception ex)
            {
                return context.Error(_loc["The messageHeader wrong json format! {0}", ex.Message]);
            }

            var query = header["query"];
            if (query != null)
            {
                return await QueryAnalize(context, query);
            }
    
            
            var objectType = header["type"];
            if (objectType == null)
            {
                return context.Error(_loc["The messageHeader do not contain key <type>"]);
            }
            if (string.IsNullOrEmpty(objectType.ScalarValue))
            {
                return context.Error(_loc["The type is empty"]);
            }
            this.objectType = objectType.ScalarValue;
            
            
            var objectId = header["id"];
                
            if (objectId == null)
            {
                return context.Error(_loc["The messageHeader do not contain key <id>"]);
            }
            if (string.IsNullOrEmpty(objectId.ScalarValue))
            {
                return context.Error(_loc["The id is empty"]);
            }
            this.objectId = objectId.ScalarValue;

            if (string.IsNullOrEmpty(message))
            {
                return context.Error(_loc["The message is empty"]);
            }

            if (context.Analizers.TryGetValue(this.objectType.ToLower(),out ObjectAnalizer analizer))
            {
                return await analizer.AnalyzeAsync(context);
            }
            else
            {
                return context.Error(_loc["Не знайдено аналізатор для типу {0}", this.objectType]);
            }
        }


    }
}
