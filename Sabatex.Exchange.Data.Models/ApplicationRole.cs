using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Sabatex.Exchange.Data.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole():base(){}
    public ApplicationRole(string role):base(role) { }

    public const string AdministratorRole = "Administrator";
    public const string ClientRole= "Client";

}
