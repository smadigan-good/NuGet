using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Represents a package repository that implements a dependency provider. 
    /// </summary>
    public class FallbackRepository : PackageRepositoryBase, IDependencyResolver, IServiceBasedRepository, ILatestPackageLookup, IOperationAwareRepository
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

        public override bool SupportsPrereleasePackages
        {
            get
            {
                return _primaryRepository.SupportsPrereleasePackages;
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

        public override IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            // Use the primary repository to look up dependencies. Fallback to the aggregate repository only if we can't find a package here.
            return _primaryRepository.ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion) ??
                _dependencyResolver.ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion);
        }

        public override IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions)
        {
            return _primaryRepository.Search(searchTerm, targetFrameworks, allowPrereleaseVersions);
        }

        public override IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            return _primaryRepository.FindPackagesById(packageId);
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

        public override IPackage FindPackage(string packageId, SemanticVersion version)
        {
            return _primaryRepository.FindPackage(packageId, version);
        }

        public override bool Exists(string packageId, SemanticVersion version)
        {
            return _primaryRepository.Exists(packageId, version);
        }

        public bool TryFindLatestPackageById(string id, out SemanticVersion latestVersion)
        {
            var latestPackageLookup = _primaryRepository as ILatestPackageLookup;
            if (latestPackageLookup != null)
            {
                return latestPackageLookup.TryFindLatestPackageById(id, out latestVersion);
            }

            latestVersion = null;
            return false;
        }

        public bool TryFindLatestPackageById(string id, bool includePrerelease, out IPackage package)
        {
            var latestPackageLookup = _primaryRepository as ILatestPackageLookup;
            if (latestPackageLookup != null)
            {
                return latestPackageLookup.TryFindLatestPackageById(id, includePrerelease, out package);
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
