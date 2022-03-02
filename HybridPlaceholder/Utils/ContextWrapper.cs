namespace HybridPlaceholder.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using Models;
    using Sitecore.LayoutService.Mvc.Routing;

    public class ContextWrapper
    {
        private readonly IRouteMapper routeMapper;
        
        public ContextWrapper(IRouteMapper routeMapper)
        {
            this.routeMapper = routeMapper;
        }
        
        private HttpContextBase Current
        {
            get
            {
                var httpContext = HttpContext.Current;
                return httpContext == null ? null : new HttpContextWrapper(httpContext);
            }
        }

        protected virtual HttpRequestBase Request => this.Current?.Request;

        private string GetQueryStringParameter(string key)
        {
            return this.Request?.QueryString?.Get(key) ?? string.Empty;
        }

        public  bool IsLayoutServiceRoute => routeMapper.IsLayoutServiceRoute(this.Current);

        public  void SetHybridPlaceholderData(Guid uid, string placeholderName, bool useSsr)
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
        
        public Dictionary<Guid, HybridPlaceholderData> GetHybridPlaceholderData()
        {
            return this.Current.Items.Contains("HybridPlaceholderData")
                ? (Dictionary<Guid, HybridPlaceholderData>) this.Current.Items["HybridPlaceholderData"]
                : new Dictionary<Guid, HybridPlaceholderData>();
        }

        public  bool IsHybridPlaceholder
        {
            get
            {
                bool.TryParse(this.GetQueryStringParameter("isHybridPlaceholder"), out var isHybridPlaceholder);
                return isHybridPlaceholder;
            }
        }
    }
}