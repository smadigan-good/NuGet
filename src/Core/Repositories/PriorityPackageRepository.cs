using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet
{
    public class PriorityPackageRepository : PackageRepositoryBase, IOperationAwareRepository
    {
        private readonly IPackageRepository _primaryRepository;
        private readonly IPackageRepository _secondaryRepository;

        public PriorityPackageRepository(IPackageRepository primaryRepository, IPackageRepository secondaryRepository)
        {
            if (primaryRepository == null)
            {
                throw new ArgumentNullException("primaryRepository");
            }

            if (secondaryRepository == null)
            {
                throw new ArgumentNullException("secondaryRepository");
            }

            _primaryRepository = primaryRepository;
            _secondaryRepository = secondaryRepository;
        }

        internal IPackageRepository PrimaryRepository
        {
            get { return _primaryRepository; }
        }

        internal IPackageRepository SecondaryRepository
        {
            get { return _secondaryRepository; }
        }

        public override string Source
        {
            get { return _primaryRepository.Source; }
        }

        public override IEnumerable<IPackage> GetPackages()
        {
            return _primaryRepository.GetPackages();
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            bool packageExists = _primaryRepository.Exists(packageId, version);
            if (!packageExists)
            {
                packageExists = _secondaryRepository.Exists(packageId, version);
            }

            return packageExists;
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            return _primaryRepository.GetPackage(packageId, version) ?? _secondaryRepository.GetPackage(packageId, version);
        }

        public override IEnumerable<IPackage> GetPackages(string packageId)
        {
            IEnumerable<IPackage> packages = _primaryRepository.GetPackages(packageId);
            if (packages.IsEmpty())
            {
                packages = _secondaryRepository.GetPackages(packageId);
            }

            return packages.Distinct().AsQueryable();
        }

        public override IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            return DisposableAction.All(_primaryRepository.StartOperation(operation, mainPackageId, mainPackageVersion),
                _secondaryRepository.StartOperation(operation, mainPackageId, mainPackageVersion));
        }
    }
}
