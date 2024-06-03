namespace ObjectsExchange.Client.Identity.Models;

/// <summary>
/// User info from identity endpoint to establish claims.
/// </summary>
public class UserInfo
{
    public required string UserId { get; set; }
    /// <summary>
    /// The email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// A value indicating whether the email has been confirmed yet.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// The list of claims for the user.
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = [];
    public required string[] Roles { get; set; }
}
