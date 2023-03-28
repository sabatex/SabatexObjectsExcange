using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Text.Json.Serialization;

namespace Sabatex.ObjectsExchange.Client.Authentication
{
  public class ApplicationUserAccount:RemoteUserAccount
  {
    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = Array.Empty<string>();
  }
}
