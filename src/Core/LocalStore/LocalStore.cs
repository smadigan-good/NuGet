using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    // TODO: Write this
    public class LocalStore : LocalPackageRepository2, ILocalStore
    {

        public LocalStore(string physicalPath)
            : this(physicalPath, enableCaching: true)
        {
        }

        public LocalStore(string physicalPath, bool enableCaching)
            : this(new DefaultPackagePathResolver(physicalPath),
                   new PhysicalFileSystem(physicalPath),
                   enableCaching)
        {
        }

        public LocalStore(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(pathResolver, fileSystem, enableCaching: true)
        {
        }

        public LocalStore(IPackagePathResolver pathResolver, IFileSystem fileSystem, bool enableCaching)
            : base(pathResolver, fileSystem)
        {

        }

        public bool IsReferenced(string packageId, SemanticVersion version)
        {
            throw new NotImplementedException();
        }

        public bool IsSolutionReferenced(string packageId, SemanticVersion version)
        {
            throw new NotImplementedException();
        }

        public void RegisterRepository(string path)
        {
            throw new NotImplementedException();
        }

        public void UnregisterRepository(string path)
        {
            throw new NotImplementedException();
        }

        public void AddPackage(string packageId, SemanticVersion version, bool developmentDependency, System.Runtime.Versioning.FrameworkName targetFramework)
        {
            throw new NotImplementedException();
        }

        public System.Runtime.Versioning.FrameworkName GetPackageTargetFramework(string packageId)
        {
            throw new NotImplementedException();
        }

    }
}
