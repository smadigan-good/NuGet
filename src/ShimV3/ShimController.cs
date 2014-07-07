using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace NuGet.ShimV3
{
    /// <summary>
    /// Contains the shim entry points.
    /// </summary>
    internal class ShimController : IShimController
    {
        private List<Tuple<string, InterceptDispatcher>> _dispatchers;
        private IPackageSourceProvider _sourceProvider;

        public ShimController()
        {

        }

        public void Enable(IPackageSourceProvider sourceProvider)
        {
            if (sourceProvider == null)
            {
                throw new ArgumentNullException("sourceProvider");
            }

            _sourceProvider = sourceProvider;

            CreateDispatchers();

            // add handlers to the Core shim
            HttpShim.Instance.SetDataServiceRequestHandler(ShimDataService);
            HttpShim.Instance.SetWebRequestHandler(ShimResponse);
        }

        public void UpdateSources()
        {
            if (_sourceProvider != null)
            {
                CreateDispatchers();
            }
        }

        public void Disable()
        {
            _sourceProvider = null;
            _dispatchers = null;

            // remove all handlers
            HttpShim.Instance.ClearHandlers();
        }

        /// <summary>
        /// Create the dispatchers for v3 urls
        /// </summary>
        private void CreateDispatchers()
        {
            _dispatchers = new List<Tuple<string, InterceptDispatcher>>(1);

            foreach(var source in _sourceProvider.LoadPackageSources())
            {
                if (source.IsEnabled && UseShim(source.Source))
                {
                    _dispatchers.Add(new Tuple<string, InterceptDispatcher>(source.Source, new InterceptDispatcher(source.Source)));
                }
            }
        }

        public WebResponse ShimResponse(WebRequest request)
        {
            Debug.Assert(request != null);

            ShimDebugLogger.Log("Request: " + request.RequestUri.AbsoluteUri);

            foreach (var dispatcher in _dispatchers)
            {
                if (request.RequestUri.AbsoluteUri.StartsWith(dispatcher.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    using (var context = new ShimCallContext(request))
                    {
                        Task t = dispatcher.Item2.Invoke(context);
                        t.Wait();
                        var stream = context.Data;

                        return new ShimWebResponse(stream, request.RequestUri, context.ResponseContentType);
                    }
                }
            }

            ShimDebugLogger.Log("Ignoring: " + request.RequestUri.AbsoluteUri);

            return request.GetResponse();
        }

        public DataServiceClientRequestMessage ShimDataService(DataServiceClientRequestMessageArgs args)
        {
            DataServiceClientRequestMessage message = null;

            if (UseShim(args.RequestUri))
            {
                ShimDebugLogger.Log("DataService Shim: " + args.RequestUri.AbsoluteUri);

                message = new ShimDataServiceClientRequestMessage(this, args);
            }
            else
            {
                ShimDebugLogger.Log("DataService Ignoring: " + args.RequestUri.AbsoluteUri);

                message = new HttpWebRequestMessage(args);
            }

            return message;
        }

        /// <summary>
        /// True if the uri starts with a known v3 feed url
        /// </summary>
        private static bool UseShim(Uri uri)
        {
            return UseShim(uri.AbsoluteUri);
        }

        /// <summary>
        /// True if the url string starts with a known v3 feed url
        /// </summary>
        private static bool UseShim(string url)
        {
            return (url != null && url.StartsWith(ShimConstants.V3FeedUrl, StringComparison.OrdinalIgnoreCase));
        }
    }
}
