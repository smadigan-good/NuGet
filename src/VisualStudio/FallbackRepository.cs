using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Represents a package repository that implements a dependency provider. 
    /// </summary>
    public class FallbackRepository : PackageRepositoryBase, IOperationAwareRepository
    {
        private readonly IPackageRepository _primaryRepository;
        private readonly IPackageRepository _dependencyResolver;

        public FallbackRepository(IPackageRepository primaryRepository, IPackageRepository dependencyResolver)
        {
            _primaryRepository = primaryRepository;
            _dependencyResolver = dependencyResolver;
        }

        public override string Source
        {
            get { return _primaryRepository.Source; }
        }

        public override PackageSaveModes PackageSaveMode
        {
            get
            {
                return _primaryRepository.PackageSaveMode;
            }
            set
            {
                _primaryRepository.PackageSaveMode = value;
            }
        }

        internal IPackageRepository SourceRepository
        {
            get { return _primaryRepository; }
        }

        internal IPackageRepository DependencyResolver
        {
            get { return _dependencyResolver; }
        }

        public override IQueryable<IPackage> GetPackages()
        {
            return _primaryRepository.GetPackages();
        }

        public override void AddPackage(IPackage package)
        {
            _primaryRepository.AddPackage(package);
        }

        public override void RemovePackage(IPackage package)
        {
            _primaryRepository.RemovePackage(package);
        }

        public override IPackage ResolveDependency(IPackageDependency dependency, DependencyVersion dependencyVersion, bool allowPrereleaseVersions, bool preferListedPackages, IPackageConstraintProvider constraintProvider)
        {
            // Use the primary repository to look up dependencies. Fallback to the aggregate repository only if we can't find a package here.
            return _primaryRepository.ResolveDependency(dependency, dependencyVersion, allowPrereleaseVersions, preferListedPackages, constraintProvider) ??
                _dependencyResolver.ResolveDependency(dependency, dependencyVersion, allowPrereleaseVersions, preferListedPackages, constraintProvider);
        }

        public override IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks)
        {
            return _primaryRepository.Search(searchTerm, allowPrereleaseVersions, targetFrameworks);
        }

        public override IQueryable<IPackage> GetPackages(string packageId)
        {
            return _primaryRepository.GetPackages(packageId);
        }

        public override IEnumerable<IPackage> GetUpdates(
            IEnumerable<IPackageName> packages, 
            bool includePrerelease, 
            bool includeAllVersions, 
            IEnumerable<FrameworkName> targetFrameworks,
            IEnumerable<IVersionSpec> versionConstraints)
        {
            return _primaryRepository.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            return _primaryRepository.GetPackage(packageId, version);
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            return _primaryRepository.Exists(packageId, version);
        }

        //public override bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        //{
        //    var latestPackageLookup = _primaryRepository;
        //    if (latestPackageLookup != null)
        //    {
        //        return latestPackageLookup.TryGetLatestPackageVersion(id, out latestVersion);
        //    }

        //    latestVersion = null;
        //    return false;
        //}

        public override bool TryGetLatestPackage(string id, bool includePrerelease, bool includeUnlisted, out IPackage package)
        {
            var latestPackageLookup = _primaryRepository;
            if (latestPackageLookup != null)
            {
                return latestPackageLookup.TryGetLatestPackage(id, includePrerelease, out package);
            }

            package = null;
            return false;
        }

        public override IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            return SourceRepository.StartOperation(operation, mainPackageId, mainPackageVersion);
        }
    }
}
