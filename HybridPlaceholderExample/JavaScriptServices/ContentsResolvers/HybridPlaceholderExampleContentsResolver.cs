namespace HybridPlaceholderExample.JavaScriptServices.ContentsResolvers
{
    using System;
    using System.Collections.Specialized;
    using Sitecore.LayoutService.Configuration;
    using Sitecore.LayoutService.ItemRendering.ContentsResolvers;
    using Sitecore.Mvc.Presentation;

    public class HybridPlaceholderExampleContentsResolver : IRenderingContentsResolver
    {
        public object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {
            var datasource = !string.IsNullOrEmpty(rendering.DataSource)
                ? rendering.RenderingItem?.Database.GetItem(rendering.DataSource)
                : null;
            
            return new
            {
                name = datasource?.Name,
                date = DateTime.Now,
                hello = "world"
            };
        }

        public bool IncludeServerUrlInMediaUrls { get; set; }
        public bool UseContextItem { get; set; }
        public string ItemSelectorQuery { get; set; }
        public NameValueCollection Parameters { get; set; }
    }
}