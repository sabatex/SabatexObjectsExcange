// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace sabatex.Identity.UI;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class IdentityDefaultUIAttribute : Attribute
{
    public IdentityDefaultUIAttribute(Type implementationTemplate)
    {
        Template = implementationTemplate;
    }

    public Type Template { get; }
}
