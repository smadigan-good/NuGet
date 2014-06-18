using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    [Export(typeof(IPackageRepositoryProvider))]
    public class GalleryV2RepositoryProvider : IPackageRepositoryProvider
    {
        public bool IsSourceSupported(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return true;
        }

        public bool TryCreateRepository(string source, out IPackageRepository repository)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var uri = new Uri(source);

            DataServicePackageRepository repo = new DataServicePackageRepository(uri);

            repository = repo;
            return true;
        }
    }
}
