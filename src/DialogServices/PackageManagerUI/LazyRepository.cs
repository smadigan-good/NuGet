using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet.Dialog.Providers
{
    public class LazyRepository : PackageRepositoryBase, IOperationAwareRepository
    {
        private readonly Lazy<IPackageRepository> _repository;

        private IPackageRepository Repository
        {
            get
            {
                return _repository.Value;
            }
        }

        public override string Source
        {
            get
            {
                return Repository.Source;
            }
        }

        public override PackageSaveModes PackageSaveMode
        {
            get
            {
                return Repository.PackageSaveMode;
            }
            set
            {
                Repository.PackageSaveMode = value;
            }
        }

        public LazyRepository(IPackageRepositoryFactory factory, PackageSource source)
        {
            _repository = new Lazy<IPackageRepository>(() => factory.CreateRepository(source.Source));
        }

        public override IQueryable<IPackage> GetPackages()
        {
            return Repository.GetPackages();
        }

        public override void AddPackage(IPackage package)
        {
            Repository.AddPackage(package);
        }

        public override void RemovePackage(IPackage package)
        {
            Repository.RemovePackage(package);
        }

        public override IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks)
        {
            return Repository.Search(searchTerm, allowPrereleaseVersions, targetFrameworks);
        }

        public override IEnumerable<IPackage> GetUpdates(
            IEnumerable<IPackageName> 
            packages, 
            bool includePrerelease, 
            bool includeAllVersions, 
            IEnumerable<FrameworkName> targetFrameworks,
            IEnumerable<IVersionSpec> versionConstraints)
        {
            return Repository.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
        }

        //public override bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        //{
        //    return Repository.TryGetLatestPackageVersion(id, out latestVersion);
        //}

        public override bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package)
        {
            return Repository.TryGetLatestPackage(id, includePrerelease, out package);
        }

        public override IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            // Starting an operation is an action that should materialize the repository
            return Repository.StartOperation(operation, mainPackageId, mainPackageVersion);
        }

        public override bool Exists(string packageId, SemanticVersion version)
        {
            return Repository.Exists(packageId, version);
        }

        public override IPackage GetPackage(string packageId, SemanticVersion version)
        {
            return Repository.GetPackage(packageId, version);
        }

        public override IQueryable<IPackage> GetPackages(string packageId)
        {
            return Repository.GetPackages(packageId);
        }
    }
}