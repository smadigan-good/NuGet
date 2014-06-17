using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Versioning;
using NuGet.VisualStudio.Resources;

namespace NuGet.VisualStudio
{
    // TODO: Remove base class maybe?
    [Export(typeof(IPackageRepository))]
    public class VsPackageSourceRepository : PackageRepositoryBase, IOperationAwareRepository
    {
        private readonly IVsPackageSourceProvider _packageSourceProvider;
        private readonly IPackageRepositoryFactory _repositoryFactory;

        [ImportingConstructor]
        public VsPackageSourceRepository(IPackageRepositoryFactory repositoryFactory,
                                         IVsPackageSourceProvider packageSourceProvider)
        {
            if (repositoryFactory == null)
            {
                throw new ArgumentNullException("repositoryFactory");
            }

            if (packageSourceProvider == null)
            {
                throw new ArgumentNullException("packageSourceProvider");
            }
            _repositoryFactory = repositoryFactory;
            _packageSourceProvider = packageSourceProvider;
        }

        public override string Source
        {
            get
            {
                var activeRepository = GetActiveRepository();
                return activeRepository == null ? null : activeRepository.Source;
            }
        }

        public override PackageSaveModes PackageSaveMode
        {
            set { throw new NotSupportedException(); }
            get { throw new NotSupportedException(); }
        }

        public override IQueryable<IPackage> GetPackages()
        {
            var activeRepository = GetActiveRepository();
            return activeRepository == null ? Enumerable.Empty<IPackage>().AsQueryable() : activeRepository.GetPackages();
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            var activeRepository = GetActiveRepository();
            return activeRepository == null ? null : activeRepository.GetPackage(packageId, version);
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            var activeRepository = GetActiveRepository();
            return activeRepository != null ? activeRepository.Exists(packageId, version) : false;
        }

        public override void AddPackage(IPackage package)
        {
            var activeRepository = GetActiveRepository();
            if (activeRepository == null)
            {
                throw new InvalidOperationException(VsResources.NoActivePackageSource);
            }
            
            activeRepository.AddPackage(package);
        }

        public override void RemovePackage(IPackage package)
        {
            var activeRepository = GetActiveRepository();
            if (activeRepository == null)
            {
                throw new InvalidOperationException(VsResources.NoActivePackageSource);
            }
            
            activeRepository.RemovePackage(package);
        }

        public override IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks)
        {
            var activeRepository = GetActiveRepository();
            if (activeRepository == null)
            {
                return Enumerable.Empty<IPackage>().AsQueryable();
            }

            return activeRepository.Search(searchTerm, allowPrereleaseVersions, targetFrameworks);
        }

        public override object Clone()
        {
            var activeRepository = GetActiveRepository();
            
            return activeRepository == null ? this : activeRepository.Clone();
        }

        public override IQueryable<IPackage> GetPackages(string packageId)
        {
            var activeRepository = GetActiveRepository();
            if (activeRepository == null)
            {
                return Enumerable.Empty<IPackage>().AsQueryable();
            }

            return activeRepository.GetPackages(packageId);
        }

        public override IEnumerable<IPackage> GetUpdates(
            IEnumerable<IPackageName> packages, 
            bool includePrerelease, 
            bool includeAllVersions, 
            IEnumerable<FrameworkName> targetFrameworks,
            IEnumerable<IVersionSpec> versionConstraints)
        {
            var activeRepository = GetActiveRepository();
            if (activeRepository == null)
            {
                return Enumerable.Empty<IPackage>();
            }

            return activeRepository.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
        }

        //public bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        //{
        //    var latestPackageLookup = GetActiveRepository();
        //    if (latestPackageLookup != null)
        //    {
        //        // TODO: should this include pre-release at times?
        //        IPackage package = null;
        //        if (latestPackageLookup.TryGetLatestPackage(id, false, out package))
        //        {
        //            latestVersion = package.Version;
        //        }
        //    }

        //    latestVersion = null;
        //    return false;
        //}

        public override bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package)
        {
            var latestPackageLookup = GetActiveRepository();
            if (latestPackageLookup != null)
            {
                return latestPackageLookup.TryGetLatestPackage(id, includePrerelease, out package);
            }

            package = null;
            return false;
        }

        internal IPackageRepository GetActiveRepository()
        {
            if (_packageSourceProvider.ActivePackageSource == null)
            {
                return null;
            }
            return _repositoryFactory.CreateRepository(_packageSourceProvider.ActivePackageSource.Source);
        }

        public override IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            var activeRepository = GetActiveRepository();
            return activeRepository.StartOperation(operation, mainPackageId, mainPackageVersion);
        }
    }
}
