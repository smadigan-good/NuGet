using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    /// <summary>
    /// HttpShim is a singleton that provides an event OnWebRequest for modifying WebRequests before they
    /// are executed.
    /// </summary>
    public sealed class HttpShim
    {
        private static HttpShim _instance;
        private Queue<Func<DataServiceClientRequestMessage, DataServiceClientRequestMessage>> _dataServiceHandlers;
        private Queue<Func<WebRequest, WebRequest>> _webHandlers;

        internal HttpShim()
        {
            _dataServiceHandlers = new Queue<Func<DataServiceClientRequestMessage, DataServiceClientRequestMessage>>();
            _webHandlers = new Queue<Func<WebRequest, WebRequest>>();
        }

        /// <summary>
        ///  Static instance of the shim.
        /// </summary>
        public static HttpShim Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpShim();
                }

                return _instance;
            }
        }

        internal WebRequest ShimWebRequest(WebRequest request)
        {
            foreach(var handler in _webHandlers)
            {
                request = handler(request);
            }

            return request;
        }

        internal DataServiceClientRequestMessage ShimDataServiceRequest(DataServiceClientRequestMessage message)
        {
            foreach (var handler in _dataServiceHandlers)
            {
                message = handler(message);
            }

            return message;
        }

        public void AddWebRequestHandler(Func<WebRequest, WebRequest> handler)
        {
            _webHandlers.Enqueue(handler);
        }

        public void AddDataServiceRequestHandler(Func<DataServiceClientRequestMessage, DataServiceClientRequestMessage> handler)
        {
            _dataServiceHandlers.Enqueue(handler);
        }
    }
}
