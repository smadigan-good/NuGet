using InterceptNuGet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet
{
    public class ShimCallContext : InterceptCallContext, IDisposable
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

        public override void Log(object obj, ConsoleColor color)
        {
            Trace.WriteLine(obj);

            using (StreamWriter writer = new StreamWriter(@"m:\shim.txt", true))
            {
                writer.WriteLine(String.Format("[{0}] {1} {2}", System.Enum.GetName(typeof(ConsoleColor), color), DateTime.Now.ToString(), obj));
            }
        }

        public void Dispose()
        {
            _sem.Dispose();
        }
    }
}
