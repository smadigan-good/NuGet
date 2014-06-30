using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class ShimWebResponse : WebResponse
    {
        private Stream _stream;
        private Uri _uri;
        private string _contentType;

        public ShimWebResponse(Stream stream, Uri uri, string contentType)
            : base()
        {
            _stream = stream;
            _uri = uri;
            _contentType = contentType;
        }

        public override Stream GetResponseStream()
        {
            return _stream;
        }

        public override Uri ResponseUri
        {
            get
            {
                return _uri;
            }
        }

        public override long ContentLength
        {
            get
            {
                return _stream.Length;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
