using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Security.Claims;

namespace Sabatex.ObjectsExchange.Client.Authentication;

public class ApplicationUserFactory:AccountClaimsPrincipalFactory<ApplicationUserAccount>
{
  public ApplicationUserFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
  {
  }

  public async override ValueTask<ClaimsPrincipal> CreateUserAsync(ApplicationUserAccount account, RemoteAuthenticationUserOptions options)
  {
    var user = await base.CreateUserAsync(account, options);
        if (user != null)
        {
            if (user.Identity != null)
            {
                if (user.Identity.IsAuthenticated)
                {
                    var userIdentity = (ClaimsIdentity)user.Identity;

                    foreach (var role in account.Roles)
                    {
                        userIdentity.AddClaim(new Claim("appRole", role));
                    }
                }
            }
            }

    return user;
  }

}
