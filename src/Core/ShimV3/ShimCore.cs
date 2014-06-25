using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public static class ShimCore
    {
        public static DataServiceClientRequestMessage ShimDataService(DataServiceClientRequestMessageArgs args)
        {
            DataServiceClientRequestMessage message = null;

            if (args.RequestUri.AbsoluteUri.IndexOf("v3", StringComparison.OrdinalIgnoreCase) > -1 || args.RequestUri.AbsoluteUri.IndexOf("shim", StringComparison.OrdinalIgnoreCase) > -1)
            {
                message = new ShimDataServiceClientRequestMessage(args);
            }
            else
            {
                message = new HttpWebRequestMessage(args);
            }

            return message;
        }
    }
}
