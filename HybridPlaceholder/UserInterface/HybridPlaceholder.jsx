import React, { useEffect, useState } from 'react';
import { Placeholder, withSitecoreContext } from '@sitecore-jss/sitecore-jss-react';
import { AxiosDataFetcher, RestLayoutService } from '@sitecore-jss/sitecore-jss';

const HybridPlaceholder = ({
  name,
  rendering,
  config,
  useApiHost,
  sitecoreContext,
}) => {
  const {
    route,
    pageEditing,
    hybridPlaceholderData,
    isLayoutServiceRoute,
  } = sitecoreContext;

  let itemId;
  let itemLanguage;
  if (route) {
    itemId = route.itemId;
    itemLanguage = route.itemLanguage;
  }

  let components;
  if (rendering
    && rendering.placeholders
    && rendering.placeholders[name]) {
    // Get all the components of the current placeholder.
    components = rendering.placeholders[name];
  }

  const [isFetched, setIsFetched] = useState(false);

  const isServer = () => !(typeof window !== 'undefined' && window.document);

  const getQueryString = params => Object.keys(params)
    .map(k => `${encodeURIComponent(k)}=${encodeURIComponent(String(params[k]))}`)
    .join('&');

  const resolveUrl = (urlBase, params = {}) => {
    if (!urlBase) {
      throw new RangeError('url must be a non-empty string');
    }

    if (isServer()) {
      const url = new URL(urlBase);
      for (const key in params) {
        if ({}.hasOwnProperty.call(params, key)) {
          url.searchParams.append(key, String(params[key]));
        }
      }
      return url.toString();
    }

    const qs = getQueryString(params);
    return urlBase.indexOf('?') !== -1 ? `${urlBase}&${qs}` : `${urlBase}?${qs}`;
  };

  const dataFetcher = (url, data) => {
    // By having a custom data dataFetcher we can add extra querystring params.
    const querystringParams = {
      isHybridPlaceholder: true,
      hasHybridSsr: !isLayoutServiceRoute,
      hybridLocation: (window.location.pathname + window.location.search),
    };
    return new AxiosDataFetcher().fetch(resolveUrl(url, querystringParams), data);
  };

  const layoutService = new RestLayoutService({
    // If no apiHost is used the current domain will be used.
    apiHost: useApiHost ? config.sitecoreApiHost : undefined,
    apiKey: config.sitecoreApiKey,
    siteName: config.jssAppName,
    dataFetcherResolver: () => dataFetcher,
  });

  const fetchPlaceholder = placeholderName => layoutService.fetchPlaceholderData(
    placeholderName,
    itemId,
    itemLanguage,
  );

  // Check if there are any components which require the async second request
  // If there are it will return the current placeholder name and shouldFetch will be true.
  // It will also set isLoaded to false for components which will be fetched.
  const applyHybridPlaceholderData = () => {
    let placeholderName;
    let shouldFetch = false;
    if (!pageEditing
      && hybridPlaceholderData
      && Object.keys(hybridPlaceholderData).length !== 0) {
      if (Array.isArray(components)) {
        components.forEach(component => {
          // Check if the component has Hybrid Placeholder data.
          if (component.componentName
            && Object.prototype.hasOwnProperty.call(hybridPlaceholderData, component.uid)) {
            // Get the name of the placeholder which should be fetched.
            placeholderName = hybridPlaceholderData[component.uid].placeholderName;
            let { useSsr } = hybridPlaceholderData[component.uid];
            if (useSsr === null || useSsr === undefined) {
              useSsr = false;
            }
            if (isLayoutServiceRoute || !useSsr) {
              // Only set shouldFetch to true if it's a isLayoutServiceRoute or if useSsr is false.
              shouldFetch = true;
            }
            if (component.fields) {
              // Set isLoaded to false for components which will be fetched.
              // eslint-disable-next-line no-param-reassign
              component.fields.isLoaded = !isLayoutServiceRoute && useSsr;
            }
          }
        });
      }
    }
    return { placeholderName, shouldFetch };
  };

  // Update the isLoaded property for each component which uses the Hybrid Placeholder.
  const setIsLoaded = isLoaded => {
    if (Array.isArray(components)) {
      components.forEach(component => {
        if (component.componentName
          && Object.prototype.hasOwnProperty.call(hybridPlaceholderData, component.uid)) {
          if (component.fields) {
            // eslint-disable-next-line no-param-reassign
            component.fields.isLoaded = isLoaded;
          }
        }
      });
    }
  };

  // Merge 2 objects recursive that will work with nested structures and skip the values that are null.
  const merge = (dst, src) => {
    Object.keys(src).forEach(key => {
      if (!dst[key]) {
        // eslint-disable-next-line no-param-reassign
        dst[key] = src[key];
      } else if (typeof src[key] === 'object' && src[key] !== null && typeof dst[key] === 'object' && dst[key] !== null) {
        // eslint-disable-next-line no-param-reassign
        merge(dst[key], src[key]);
      }
    });
  };

  // Update elements by merging them.
  const updateElements = (currentElements, updatedElements) => {
    if (Array.isArray(currentElements) && Array.isArray(updatedElements)) {
      currentElements.forEach((currentElement, index, elements) => {
        if (currentElement) {
          const updatedElement = updatedElements.find(element => (element && element.uid === currentElement.uid));
          if (updatedElement) {
            // Merge the properties from  updatedElement into currentElement which are not null.
            merge(currentElement, updatedElement);
            // eslint-disable-next-line no-param-reassign
            elements[index] = currentElement;
          }
        }
      });
    }
  };

  // Run this code when navigating to a new page.
  useEffect(() => {
    // Get the current placeholder name and if we should fetch the async data in a second request.
    const { placeholderName, shouldFetch } = applyHybridPlaceholderData();
    if (shouldFetch) {
      setIsFetched(false);
      // Get the async data.
      fetchPlaceholder(placeholderName)
        .then(result => {
          // Merge the async data from the second request with the data fetched in the first request.
          updateElements(components, result.elements);
          setIsLoaded(true);
          setIsFetched(true);
        }).catch(error => {
          console.log('Hybrid Placeholder error', error);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [itemId]);

  if (!isFetched) {
    // Check which components will execute the async second request.
    // Those will show a loader by setting the isLoaded prop to false.
    applyHybridPlaceholderData();
  }

  return (
    <Placeholder name={name} rendering={rendering} />
  );
};

HybridPlaceholder.defaultProps = {
  name: undefined,
  rendering: undefined,
  config: {},
  useApiHost: false,
  sitecoreContext: {},
};

export default withSitecoreContext()(HybridPlaceholder);
