namespace HybridPlaceholder.Pipelines
{
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.LayoutService.Mvc.Routing;
    using Sitecore.Mvc.Pipelines.Response.RenderPlaceholder;
    using Sitecore.Mvc.Presentation;
    using Utils;

    public class PerformHybridRendering : PerformRendering
    {
        private readonly ContextWrapper contextWrapper;
        
        public PerformHybridRendering(RendererCache rendererCache, IRouteMapper routeMapper) : base(rendererCache)
        {
            this.contextWrapper = new ContextWrapper(routeMapper);
        }

        protected override IEnumerable<Rendering> GetRenderings(string placeholderName, RenderPlaceholderArgs args)
        {
            var isHybridPlaceholder = this.contextWrapper.IsHybridPlaceholder;
            var hasHybridSsr = this.contextWrapper.HasHybridSsr;
            var renderings = base.GetRenderings(placeholderName, args);
            renderings = renderings.Where(x =>
            {
                if (!isHybridPlaceholder)
                {
                    return true;
                }

                // Only return renderings during the Hybrid Placeholder request that have this field enabled.
                // So renderings don't run twice which don't need to.
                var enableHybridRenderingField = x?.RenderingItem?.InnerItem?.Fields?["Enable Hybrid Rendering"];
                var enableHybridRendering = enableHybridRenderingField != null && enableHybridRenderingField.Value == "1";

                // Check if SSR already happend. If it did only return the renderings which have SSR disabled.
                var disableHybridSsrRenderingField = x?.RenderingItem?.InnerItem?.Fields?["Disable Hybrid SSR"];
                var disableHybridSsr = disableHybridSsrRenderingField != null && disableHybridSsrRenderingField.Value == "1";

                return enableHybridRendering && (!hasHybridSsr || disableHybridSsr);

            });
            return renderings;
        }
    }
}