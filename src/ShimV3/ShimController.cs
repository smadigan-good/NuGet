using InterceptNuGet;
using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class ShimController : IShimController
    {
        // private const string BaseAddress = "http://nuget3.blob.core.windows.net/feed/resolver";
        // private const string SearchBaseAddress = "http://nuget-dev-0-search.cloudapp.net/search/query";
        // private const string PassThroughAddress = "http://nuget.org";

        private List<Tuple<string, InterceptDispatcher>> _dispatchers;
        private IPackageSourceProvider _sourceProvider;

        public ShimController()
        {

        }

        public void Enable(IPackageSourceProvider sourceProvider)
        {
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

        private void CreateDispatchers()
        {
            _dispatchers = new List<Tuple<string, InterceptDispatcher>>();

            foreach(var source in _sourceProvider.LoadPackageSources())
            {
                if (source.IsEnabled)
                {
                    _dispatchers.Add(new Tuple<string, InterceptDispatcher>(source.Source, new InterceptDispatcher(source.Source)));
                }
            }
        }


        public WebResponse ShimResponse(WebRequest request)
        {
            if (UseShim(request.RequestUri))
            {
                using (var context = new ShimCallContext(request))
                {
                    foreach(var dispatcher in _dispatchers)
                    {
                        if (request.RequestUri.AbsoluteUri.StartsWith(dispatcher.Item1, StringComparison.OrdinalIgnoreCase) || request.RequestUri.AbsoluteUri.Equals(dispatcher.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            Task t = dispatcher.Item2.Invoke(context);
                            t.Wait();
                            var stream = context.Data;

                            return new ShimWebResponse(stream, request.RequestUri, context.ResponseContentType);
                        }
                    }
                }
            }

            return request.GetResponse();
        }

        public DataServiceClientRequestMessage ShimDataService(DataServiceClientRequestMessageArgs args)
        {
            DataServiceClientRequestMessage message = null;

            if (UseShim(args.RequestUri))
            {
                message = new ShimDataServiceClientRequestMessage(this, args);
            }
            else
            {
                message = new HttpWebRequestMessage(args);
            }

            return message;
        }

        private static bool UseShim(Uri uri)
        {
            return (uri.AbsoluteUri.StartsWith("https://preview-api.nuget.org/ver3/", StringComparison.OrdinalIgnoreCase));
        }
    }
}
