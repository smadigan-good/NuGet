using InterceptNuGet;
using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public static class ShimCore
    {
        static string BaseAddress = "http://nuget3.blob.core.windows.net/feed/resolver";
        static string SearchBaseAddress = "http://nuget-dev-0-search.cloudapp.net/search/query";
        static string PassThroughAddress = "http://nuget.org";
        static InterceptDispatcher _dispatcher = new InterceptDispatcher(BaseAddress, SearchBaseAddress, PassThroughAddress);

        public static IODataResponseMessage ShimResponseMessage(WebRequest request)
        {
            if (UseShim(request.RequestUri))
            {
                return new ShimResponseMessage(ShimResponse(request));
            }
            else
            {
                return new ShimResponseMessage(request.GetResponse());
            }
        }

        public static WebResponse ShimResponse(WebRequest request)
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

        public static DataServiceClientRequestMessage ShimDataService(DataServiceClientRequestMessageArgs args)
        {
            DataServiceClientRequestMessage message = null;

            if (UseShim(args.RequestUri))
            {
                message = new ShimDataServiceClientRequestMessage(args);
            }
            else
            {
                message = new HttpWebRequestMessage(args);
            }

            return message;
        }

        private static bool UseShim(Uri uri)
        {
            //return (uri.AbsoluteUri.IndexOf("v3", StringComparison.OrdinalIgnoreCase) > -1 || uri.AbsoluteUri.IndexOf("shim", StringComparison.OrdinalIgnoreCase) > -1);
            return true;
        }
    }
}
