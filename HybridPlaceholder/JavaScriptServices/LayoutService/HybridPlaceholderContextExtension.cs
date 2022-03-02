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
    using Utils;

    public class HybridPlaceholderContextExtension : JssGetLayoutServiceContextProcessor
    {
        private readonly ContextWrapper contextWrapper;

        public HybridPlaceholderContextExtension(IConfigurationResolver configurationResolver, IRouteMapper routeMapper) : base(configurationResolver)
        {
            this.contextWrapper = new ContextWrapper(routeMapper);
        }

        protected override void DoProcess(GetLayoutServiceContextArgs args, AppConfiguration application)
        {
            args.ContextData.Add("isLayoutServiceRoute", this.contextWrapper.IsLayoutServiceRoute);
            args.ContextData.Add("hybridPlaceholderData", this.contextWrapper.GetHybridPlaceholderData());
        }
    }
}