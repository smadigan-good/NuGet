using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Test.Mocks
{
    public class MockServiceBasePackageRepository : MockPackageRepository
    {
        public override IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<System.Runtime.Versioning.FrameworkName> targetFrameworks, IEnumerable<IVersionSpec> versionConstraints)
        {
            // only keep the latest version of each package Id to mimic the behavior of nuget.org GetUpdates() service method
            packages = packages.OrderByDescending(p => p.Version).Distinct(PackageEqualityComparer.Id);

            return this.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
        }
    }
}