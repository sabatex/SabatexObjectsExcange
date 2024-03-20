namespace ObjectsExchange.Models;

public class ApplicationAuthenticationState
{
    public bool IsAuthenticated { get; set; }
    public string Name { get; set; }
    public IEnumerable<ApplicationClaim> Claims { get; set; }

}
