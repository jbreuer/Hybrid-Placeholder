namespace HybridPlaceholder.JavaScriptServices.ContentsResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Sitecore.LayoutService.Configuration;
    using Sitecore.LayoutService.ItemRendering.ContentsResolvers;
    using Sitecore.LayoutService.Mvc.Routing;
    using Sitecore.Mvc.Presentation;
    using Utils;

    public abstract class HybridRenderingContentsResolver<TContent, TRenderingParameters> : IRenderingContentsResolver
    {
        private readonly ContextWrapper contextWrapper;

        protected HybridRenderingContentsResolver(IRouteMapper routeMapper)
        {
            this.contextWrapper = new ContextWrapper(routeMapper);
        }
        
        public object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var disableHybridSsrRenderingField = rendering.RenderingItem?.InnerItem?.Fields["Disable Hybrid SSR"];
            var disableHybridSsr = disableHybridSsrRenderingField != null && disableHybridSsrRenderingField.Value == "1" && !Sitecore.Context.PageMode.IsExperienceEditor;
            
            // Store the data the Hybrid Placeholder needs in the frontend.
            this.contextWrapper.SetHybridPlaceholderData(rendering.UniqueId, rendering.Placeholder, !disableHybridSsr);
            
            var content = default(TContent);
            var renderingParameters = default(TRenderingParameters);
            
            if (!this.contextWrapper.IsLayoutServiceRoute || !this.contextWrapper.IsHybridPlaceholder)
            {
                // Only runs once. If it's SSR or when it's not called from the Hybrid Placeholder.
                (content, renderingParameters) = ResolveDefaultContents(rendering, renderingConfig);
            }
            if ((!this.contextWrapper.IsLayoutServiceRoute && !disableHybridSsr) || this.contextWrapper.IsHybridPlaceholder)
            {
                // Only runs once. If it's SSR or when the Hybrid Placeholder fetches the async data.
                (content, renderingParameters) = ResolveAsyncContents(content, renderingParameters, rendering, renderingConfig);
            }
            
            return JToken.FromObject(content, new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public bool IncludeServerUrlInMediaUrls { get; set; }
        public bool UseContextItem { get; set; }
        public string ItemSelectorQuery { get; set; }
        public NameValueCollection Parameters { get; set; }

        
        
        protected abstract (TContent content, TRenderingParameters renderingParameters) ResolveDefaultContents(Rendering rendering, IRenderingConfiguration renderingConfig);
        
        protected abstract (TContent content, TRenderingParameters renderingParameters) ResolveAsyncContents(TContent content, TRenderingParameters renderingParameters, Rendering rendering, IRenderingConfiguration renderingConfig);
    }
}