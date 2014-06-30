using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class ShimResponseMessage : IODataResponseMessage
    {
        public HttpWebResponse WebReponse { get; private set; }

        public ShimResponseMessage(WebResponse response)
        {
            WebReponse = response as HttpWebResponse;
        }

        public ShimResponseMessage(HttpWebResponse response)
        {
            WebReponse = response;
        }


        public string GetHeader(string headerName)
        {
            return WebReponse.Headers.Get(headerName);
        }

        public Stream GetStream()
        {
            return WebReponse.GetResponseStream();
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();

                foreach (var header in WebReponse.Headers.AllKeys)
                {
                    headers.Add(new KeyValuePair<string, string>(header, WebReponse.Headers.Get(header)));
                }

                return headers;
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            WebReponse.Headers.Set(headerName, headerValue);
        }

        public int StatusCode
        {
            get
            {
                return (int)WebReponse.StatusCode;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
