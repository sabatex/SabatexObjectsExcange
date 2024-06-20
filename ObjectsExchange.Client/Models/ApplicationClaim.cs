namespace ObjectsExchange.Models;

public class ApplicationClaim
{
    public string Type { get; set; } = default!;
    public string Value { get; set; } = default!;
    public const string AdministratorRole = "Administrator";
    public const string ClientRole = "Client";
    public const string ClientUserRole = "ClientUser";

}