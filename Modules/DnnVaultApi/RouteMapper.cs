// <copyright file="RouteMapper.cs" company="Dowdian SRL">
// Copyright (c) Dowdian SRL. All rights reserved.
// </copyright>

namespace Dowdian.Modules.DnnVaultApi.Controllers
{
    /// <summary>
    /// RouteMapper
    /// THIS ONE IS FOR THE API
    /// </summary>
    public class RouteMapper : DotNetNuke.Web.Api.IServiceRouteMapper
    {
        /// <summary>
        /// RegisterRoutes
        /// </summary>
        /// <param name="mapRouteManager">DotNetNuke.Web.Api.IMapRoute mapRouteManager</param>
        public void RegisterRoutes(DotNetNuke.Web.Api.IMapRoute mapRouteManager)
        {
            // http://DowdianDnnVaultApi.domain.com/DesktopModules/DnnVaultApi/API/Controller/Action
            mapRouteManager.MapHttpRoute("DnnVaultApi", "default", "{controller}/{action}", new[] { "Dowdian.Modules.DnnVaultApi.Controllers" });
        }
    }
}