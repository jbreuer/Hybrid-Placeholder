namespace HybridPlaceholderExample.JavaScriptServices.ContentsResolvers
{
    using System;
    using System.Threading;
    using HybridPlaceholder.JavaScriptServices.ContentsResolvers;
    using Models;
    using Sitecore.LayoutService.Configuration;
    using Sitecore.LayoutService.Mvc.Routing;
    using Sitecore.Mvc.Presentation;

    public class HybridPlaceholderExampleContentsResolver : HybridRenderingContentsResolver<HybridExample, object>
    {
        public HybridPlaceholderExampleContentsResolver(IRouteMapper routeMapper) : base(routeMapper)
        {
        }

        protected override (HybridExample content, object renderingParameters) ResolveDefaultContents(
            Rendering rendering,
            IRenderingConfiguration renderingConfig)
        {
            var datasource = !string.IsNullOrEmpty(rendering.DataSource)
                ? rendering.RenderingItem?.Database.GetItem(rendering.DataSource)
                : null;

            var processedItem = base.ProcessItem(datasource, rendering, renderingConfig);

            var hybridExample = new HybridExample
            {
                Heading = processedItem?["Heading"],
                Text = processedItem?["Text"]
            };

            return (hybridExample, null);
        }

        protected override (HybridExample content, object renderingParameters) ResolveAsyncContents(
            HybridExample content,
            object renderingParameters, Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var hybridExample = content ?? new HybridExample();
            Thread.Sleep(2000);
            hybridExample.Date = DateTime.Now.ToString("f");

            return (hybridExample, null);
        }
    }
}