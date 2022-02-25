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

    public abstract class HybridRenderingContentsResolver<TContent, TRenderingParameters> : IRenderingContentsResolver
    {
        private readonly IRouteMapper routeMapper;

        public HybridRenderingContentsResolver(IRouteMapper routeMapper)
        {
            this.routeMapper = routeMapper;
        }
        
        public object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var disableHybridSsrRenderingField = rendering.RenderingItem?.InnerItem?.Fields["Disable Hybrid SSR"];
            var disableHybridSsr = disableHybridSsrRenderingField != null && disableHybridSsrRenderingField.Value == "1" && !Sitecore.Context.PageMode.IsExperienceEditor;
            
            // Store the data the Hybrid Placeholder needs in the frontend.
            this.SetHybridPlaceholderData(rendering.UniqueId, rendering.Placeholder, !disableHybridSsr);
            
            var content = default(TContent);
            var renderingParameters = default(TRenderingParameters);
            
            if (!this.IsLayoutServiceRoute || !this.IsHybridPlaceholder)
            {
                // Only runs once. If it's SSR or when it's not called from the Hybrid Placeholder.
                (content, renderingParameters) = ResolveDefaultContents(rendering, renderingConfig);
            }
            if ((!this.IsLayoutServiceRoute && !disableHybridSsr) || this.IsHybridPlaceholder)
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
        
        public HttpContextBase Current
        {
            get
            {
                var httpContext = HttpContext.Current;
                return httpContext == null ? null : new HttpContextWrapper(httpContext);
            }
        }
        
        public virtual HttpRequestBase Request => this.Current?.Request;
        
        public string GetQueryStringParameter(string key)
        {
            return this.Request?.QueryString?.Get(key) ?? string.Empty;
        }
        
        public bool IsLayoutServiceRoute => routeMapper.IsLayoutServiceRoute(this.Current);
        
        public void SetHybridPlaceholderData(Guid uid, string placeholderName, bool useSsr)
        {
            var hybridPlaceholderData = this.Current.Items.Contains("HybridPlaceholderData")
                ? (Dictionary<Guid, HybridPlaceholderData>) this.Current.Items["HybridPlaceholderData"]
                : new Dictionary<Guid, HybridPlaceholderData>();

            if (hybridPlaceholderData.ContainsKey(uid))
            {
                hybridPlaceholderData[uid] = new HybridPlaceholderData
                {
                    PlaceholderName = placeholderName,
                    UseSsr = useSsr
                };
            }
            else
            {
                hybridPlaceholderData.Add(uid, new HybridPlaceholderData
                {
                    PlaceholderName = placeholderName,
                    UseSsr = useSsr
                });
            }

            this.Current.Items["HybridPlaceholderData"] = hybridPlaceholderData;
        }

        public bool IsHybridPlaceholder
        {
            get
            {
                bool.TryParse(this.GetQueryStringParameter("isHybridPlaceholder"), out var isHybridPlaceholder);
                return isHybridPlaceholder;
            }
        }
        
        protected abstract (TContent content, TRenderingParameters renderingParameters) ResolveDefaultContents(Rendering rendering, IRenderingConfiguration renderingConfig);
        
        protected abstract (TContent content, TRenderingParameters renderingParameters) ResolveAsyncContents(TContent content, TRenderingParameters renderingParameters, Rendering rendering, IRenderingConfiguration renderingConfig);
    }
}