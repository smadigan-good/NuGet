using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NuGet.GalleryV3
{
    public interface IGalleryV3Repository :
                IPackageRepository,
                IHttpClientEvents,
                IServiceBasedRepository,
                ICloneableRepository,
                ICultureAwareRepository,
                IOperationAwareRepository,
                IPackageLookup,
                ILatestPackageLookup,
                IWeakEventListener
    {



    }
}
