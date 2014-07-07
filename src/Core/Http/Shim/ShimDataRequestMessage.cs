﻿using Microsoft.Data.OData;
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
    public class ShimDataRequestMessage : IODataRequestMessage
    {

        public HttpWebRequest WebRequest { get; private set; }

        private SendingRequest2EventArgs _args;

        public ShimDataRequestMessage(SendingRequest2EventArgs args)
        {
            _args = args;

            WebRequest = ShimWebHelpers.AddHeaders(HttpWebRequest.CreateHttp(_args.RequestMessage.Url), _args.RequestMessage.Headers);

            WebRequest.Method = _args.RequestMessage.Method;
        }

        public ShimDataRequestMessage(DataServiceClientRequestMessageArgs args)
        {
            WebRequest = ShimWebHelpers.AddHeaders(HttpWebRequest.CreateHttp(args.RequestUri), args.Headers);

            WebRequest.Method = args.Method;
        }

        public string GetHeader(string headerName)
        {
            return WebRequest.Headers.Get(headerName);
        }

        public Stream GetStream()
        {
            return WebRequest.GetRequestStream();
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get 
            {
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();

                foreach(var header in WebRequest.Headers.AllKeys)
                {
                    headers.Add(new KeyValuePair<string, string>(header, WebRequest.Headers.Get(header)));
                }

                return headers;
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            WebRequest.Headers.Set(headerName, headerValue);
        }

        public Uri Url
        {
            get
            {
                return WebRequest.RequestUri;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public string Method
        {
            get
            {
                return WebRequest.Method;
            }
            set
            {
                WebRequest.Method = value;
            }
        }

        //internal void ApplyChanges()
        //{
        //    _args.RequestMessage.Method = WebRequest.Method;
        //    _args.RequestMessage.Url = WebRequest.RequestUri;

        //    // Remove all headers
        //    foreach(var header in _args.RequestMessage.Headers)
        //    {
        //        _args.RequestMessage.SetHeader(header.Key, null);
        //    }

        //    // Set headers according to the web request
        //    foreach(var header in Headers)
        //    {
        //        _args.RequestMessage.SetHeader(header.Key, header.Value);
        //    }            
        //}
    }
}
