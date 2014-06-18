using System.Runtime.Versioning;

namespace NuGet
{
    public interface IPackageReferenceRepository : IPackageRepository, IMutablePackageRepository
    {
        void AddPackage(string packageId, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework);
        FrameworkName GetPackageTargetFramework(string packageId);
    }
}
