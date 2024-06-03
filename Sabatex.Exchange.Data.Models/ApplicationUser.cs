using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Sabatex.Exchange.Data.Models;

public class ApplicationUser : IdentityUser<Guid>
{


}
