﻿using System;
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
        private IDebugConsoleController _logger;
        private Guid _guid;

        public ShimCallContext(WebRequest request, IDebugConsoleController logger)
            :base()
        {
            _logger = logger;
            _request = request;
            _sem = new ManualResetEvent(false);
            _guid = Guid.NewGuid();
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

        public override void Log(string message, ConsoleColor color, TimeSpan? elapsed, int? bytes)
        {
            if (!String.IsNullOrEmpty(message) && _logger != null)
            {
                _logger.Log(message, color, elapsed, bytes, _guid);
            }
        }

        public void Dispose()
        {
            _sem.Dispose();
        }
    }
}
