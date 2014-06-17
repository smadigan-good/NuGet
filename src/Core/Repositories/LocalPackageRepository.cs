using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class LocalPackageRepository : SimpleRepository
    {
        // Adds caching to simple repository

        public LocalPackageRepository(string path)
            : this(new DefaultPackagePathResolver(path), new PhysicalFileSystem(path))
        {

        }

        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(pathResolver, fileSystem, true)
        {

        }


        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem, bool enableCaching)
            : base(pathResolver, fileSystem)
        {

        }


        public bool TryGetLatestPackageVersion(string packageId, out SemanticVersion version)
        {
            IPackage package = null;
            if (TryGetLatestPackage(packageId, true, true, out package))
            {
                version = package.Version.ToSemanticVersion();
                return true;
            }

            version = null;
            return false;
        }

    }
}
