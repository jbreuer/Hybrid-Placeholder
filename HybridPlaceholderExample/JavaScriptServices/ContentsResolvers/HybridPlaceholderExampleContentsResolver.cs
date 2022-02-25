using Sitecore.LayoutService.Mvc.Routing;

namespace HybridPlaceholderExample.JavaScriptServices.ContentsResolvers
{
    using System;
    using System.Threading;
    using HybridPlaceholder.JavaScriptServices.ContentsResolvers;
    using Models;
    using Sitecore.LayoutService.Configuration;
    using Sitecore.Mvc.Presentation;

    public class HybridPlaceholderExampleContentsResolver : HybridRenderingContentsResolver<HybridExample, object>
    {
        public HybridPlaceholderExampleContentsResolver(IRouteMapper routeMapper) : base(routeMapper)
        {
        }

        protected override (HybridExample content, object renderingParameters) ResolveDefaultContents(Rendering rendering,
            IRenderingConfiguration renderingConfig)
        {   
            var datasource = !string.IsNullOrEmpty(rendering.DataSource)
                ? rendering.RenderingItem?.Database.GetItem(rendering.DataSource)
                : null;

            var hybridExample = new HybridExample
            {
                Hello = "World 2",
                Name = datasource?["heading"]
            };
            
            return (hybridExample, null);
        }

        protected override (HybridExample content, object renderingParameters) ResolveAsyncContents(HybridExample content,
            object renderingParameters, Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var hybridExample = content ?? new HybridExample();
            Thread.Sleep(1000);
            hybridExample.Date = DateTime.Now.ToString("f");
            
            return (hybridExample, null);
        }
    }
}