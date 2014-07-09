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
        private Action<string, ConsoleColor> _logger;

        public ShimCallContext(WebRequest request, Action<string, ConsoleColor> logger)
            :base()
        {
            _logger = logger;
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

        public override void Log(object obj, ConsoleColor color)
        {
            string s = obj as string;

            if (!String.IsNullOrEmpty(s))
            {
                _logger(s, color);
            }
        }

        public void Dispose()
        {
            _sem.Dispose();
        }
    }
}
