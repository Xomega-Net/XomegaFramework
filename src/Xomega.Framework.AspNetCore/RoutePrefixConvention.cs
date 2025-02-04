// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Xomega.Framework.Services;

/// <summary>
/// Provides extension methods for adding a convention for prefixing all API endpoints with a specific route prefix.
/// </summary>
public static class RoutePrefixExtensions
{
    /// <summary>
    /// Adds a convention for prefixing all API endpoints with a specific route prefix.
    /// </summary>
    /// <param name="opts">MVC options to configure.</param>
    /// <param name="routeAttribute">The route template provider to use.</param>
    public static void UseGeneralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
    {
        opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
    }

    /// <summary>
    /// Adds a convention for prefixing all API endpoints with a specific route prefix.
    /// </summary>
    /// <param name="opts">MVC options to configure.</param>
    /// <param name="prefix">The route prefix to use.</param>
    public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix)
    {
        opts.UseGeneralRoutePrefix(new RouteAttribute(prefix));
    }
}

/// <summary>
/// A convention that adds the specified route prefix to all endpoints in application controllers.
/// </summary>
/// <param name="route">Injected route template provider.</param>
public class RoutePrefixConvention(IRouteTemplateProvider route) : IApplicationModelConvention
{
    private readonly AttributeRouteModel routePrefix = new(route);

    /// <inheritdoc/>
    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
        {
            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(routePrefix, selector.AttributeRouteModel);
            }
            else
            {
                selector.AttributeRouteModel = routePrefix;
            }
        }
    }
}
