namespace HybridPlaceholder.JavaScriptServices.LayoutService
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Models;
    using Sitecore.JavaScriptServices.Configuration;
    using Sitecore.JavaScriptServices.ViewEngine.LayoutService.Pipelines.GetLayoutServiceContext;
    using Sitecore.LayoutService.ItemRendering.Pipelines.GetLayoutServiceContext;
    using Sitecore.LayoutService.Mvc.Routing;

    public class HybridPlaceholderContextExtension : JssGetLayoutServiceContextProcessor
    {
        private readonly IRouteMapper routeMapper;

        public HybridPlaceholderContextExtension(IConfigurationResolver configurationResolver, IRouteMapper routeMapper) : base(configurationResolver)
        {
            this.routeMapper = routeMapper;
        }

        protected override void DoProcess(GetLayoutServiceContextArgs args, AppConfiguration application)
        {
            args.ContextData.Add("isLayoutServiceRoute", this.IsLayoutServiceRoute);
            args.ContextData.Add("hybridPlaceholderData", this.GetHybridPlaceholderData());
        }
        
        public HttpContextBase Current
        {
            get
            {
                var httpContext = HttpContext.Current;
                return httpContext == null ? null : new HttpContextWrapper(httpContext);
            }
        }
        
        public bool IsLayoutServiceRoute => routeMapper.IsLayoutServiceRoute(this.Current);
        
        public Dictionary<Guid, HybridPlaceholderData> GetHybridPlaceholderData()
        {
            return this.Current.Items.Contains("HybridPlaceholderData")
                ? (Dictionary<Guid, HybridPlaceholderData>) this.Current.Items["HybridPlaceholderData"]
                : new Dictionary<Guid, HybridPlaceholderData>();
        }
    }
}