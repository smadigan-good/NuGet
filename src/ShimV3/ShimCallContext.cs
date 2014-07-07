using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.ShimV3
{
    internal class ShimCallContext : InterceptCallContext, IDisposable
    {
        private WebRequest _request;
        private ManualResetEvent _sem;
        private MemoryStream _data;
        private string _contentType;

        public ShimCallContext(WebRequest request)
            :base()
        {
            _request = request;
            _sem = new ManualResetEvent(false);
        }

        public override Uri RequestUri
        {
            get
            {
                return _request.RequestUri;
            }
        }

        public override string ResponseContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        public Stream Data
        {
            get
            {
                _sem.WaitOne();
                return _data;
            }
        }

        public override Task WriteResponseAsync(byte[] data)
        {
            return Task.Run(() =>
                {
                    _data = new MemoryStream(data);
                    _sem.Set();
                });
        }

        #if DEBUG
        public override void Log(object obj, ConsoleColor color)
        {
            ShimDebugLogger.Log(String.Format(CultureInfo.InvariantCulture, "({0}) {1}", System.Enum.GetName(typeof(ConsoleColor), color), obj));
        }
        #endif

        public void Dispose()
        {
            _sem.Dispose();
        }
    }
}
