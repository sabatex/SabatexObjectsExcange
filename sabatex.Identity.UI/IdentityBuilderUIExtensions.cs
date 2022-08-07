// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using sabatex.Identity.UI;
using sabatex.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Identity;

/// <summary>
/// Default UI extensions to <see cref="IdentityBuilder"/>.
/// </summary>
public static class IdentityBuilderUIExtensions
{
    /// <summary>
    /// Adds a default, self-contained UI for Identity to the application using
    /// Razor Pages in an area named Identity.
    /// </summary>
    /// <remarks>
    /// In order to use the default UI, the application must be using <see cref="Microsoft.AspNetCore.Mvc"/>,
    /// <see cref="Microsoft.AspNetCore.StaticFiles"/> and contain a <c>_LoginPartial</c> partial view that
    /// can be found by the application.
    /// </remarks>
    /// <param name="builder">The <see cref="IdentityBuilder"/>.</param>
    /// <returns>The <see cref="IdentityBuilder"/>.</returns>
    public static IdentityBuilder AddDefaultUI(this IdentityBuilder builder)
    {
        builder.AddSignInManager();
        builder.Services
            .AddMvc()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization()
            .ConfigureApplicationPartManager(apm =>
            {

                var parts = new ConsolidatedAssemblyApplicationPartFactory().GetApplicationParts(typeof(IdentityBuilderUIExtensions).Assembly);
                foreach (var part in parts)
                {
                    apm.ApplicationParts.Add(part);
                }
            });


        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        

        builder.Services.ConfigureOptions(
            typeof(IdentityDefaultUIConfigureOptions<>)
                .MakeGenericType(builder.UserType));
        builder.Services.TryAddTransient<IEmailSender, EmailSender>();
        builder.Services.TryAddTransient<IUserActions, UserActions>();

        return builder;
    }

    private static Assembly? GetApplicationAssembly(IdentityBuilder builder)
    {
        // This is the same logic that MVC follows to find the application assembly.
        var environment = builder.Services.Where(d => d.ServiceType == typeof(IWebHostEnvironment)).ToArray();
        var applicationName = ((IWebHostEnvironment?)environment.LastOrDefault()?.ImplementationInstance)
            ?.ApplicationName;

        if (applicationName == null)
        {
            return null;
        }
        var appAssembly = Assembly.Load(applicationName);
        return appAssembly;
    }

}
