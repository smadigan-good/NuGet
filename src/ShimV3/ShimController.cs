using InterceptNuGet;
using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class ShimController : IShimController
    {
        private const string BaseAddress = "http://nuget3.blob.core.windows.net/feed/resolver";
        private const string SearchBaseAddress = "http://nuget-dev-0-search.cloudapp.net/search/query";
        private const string PassThroughAddress = "http://nuget.org";

        private InterceptDispatcher _dispatcher;
        private IPackageSourceProvider _sourceProvider;

        public ShimController()
        {

        }

        public void Enable(IPackageSourceProvider sourceProvider)
        {
            _sourceProvider = sourceProvider;

            _dispatcher = CreateDispatcher();

            // add handlers to the Core shim
            HttpShim.Instance.SetDataServiceRequestHandler(ShimDataService);
            HttpShim.Instance.SetWebRequestHandler(ShimResponse);
        }

        public void UpdateSources()
        {
            if (_sourceProvider != null)
            {
                _dispatcher = CreateDispatcher();
            }
        }

        public void Disable()
        {
            _sourceProvider = null;
            _dispatcher = null;

            // remove all handlers
            HttpShim.Instance.ClearHandlers();
        }

        private static InterceptDispatcher CreateDispatcher()
        {
            return new InterceptDispatcher(BaseAddress, SearchBaseAddress, PassThroughAddress);
        }


        public WebResponse ShimResponse(WebRequest request)
        {
            if (UseShim(request.RequestUri))
            {
                using (var context = new ShimCallContext(request))
                {
                    Task t = _dispatcher.Invoke(context);
                    t.Wait();
                    var stream = context.Data;

                    return new ShimWebResponse(stream, request.RequestUri, context.ResponseContentType);
                }
            }
            else
            {
                return request.GetResponse();
            }
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
