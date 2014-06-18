using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet
{
    /// <summary>
    /// This repository implementation keeps track of packages that are referenced in a project but
    /// it also has a reference to the repository that actually contains the packages. It keeps track
    /// of packages in an xml file at the project root (packages.xml).
    /// </summary>
    public class PackageReferenceRepository : PackageRepositoryBase, IPackageReferenceRepository, IPackageConstraintProvider
    {
        private readonly PackageReferenceFile _packageReferenceFile;
        private readonly string _fullPath;

        public PackageReferenceRepository(
            IFileSystem fileSystem, 
            string projectName, 
            ISharedPackageRepository sourceRepository)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (sourceRepository == null)
            {
                throw new ArgumentNullException("sourceRepository");
            }

            _packageReferenceFile = new PackageReferenceFile(
                fileSystem, Constants.PackageReferenceFile, projectName);

            _fullPath = _packageReferenceFile.FullPath;
            SourceRepository = sourceRepository;
        }

        public PackageReferenceRepository(
            string configFilePath,
            ISharedPackageRepository sourceRepository)
        {
            if (String.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "configFilePath");
            }

            if (sourceRepository == null)
            {
                throw new ArgumentNullException("sourceRepository");
            }

            _packageReferenceFile = new PackageReferenceFile(configFilePath);
            _fullPath = configFilePath;
            SourceRepository = sourceRepository;
        }

        public override string Source
        {
            get
            {
                return Constants.PackageReferenceFile;
            }
        }

        public override PackageSaveModes PackageSaveMode
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        private ISharedPackageRepository SourceRepository
        {
            get;
            set;
        }

        private string PackageReferenceFileFullPath
        {
            get
            {
                return _fullPath;
            }
        }

        public PackageReferenceFile ReferenceFile
        {
            get
            {
                return _packageReferenceFile;
            }
        }

        public override IQueryable<IPackage> GetPackages()
        {
            return GetPackagesCore().AsQueryable();
        }

        private IEnumerable<IPackage> GetPackagesCore()
        {
            return _packageReferenceFile.GetPackageReferences()
                                        .Select(GetPackage)
                                        .Where(p => p != null);
        }

        public void AddPackage(IPackage package)
        {
            AddPackage(package.Id, package.Version.ToSemanticVersion(), package.DevelopmentDependency, targetFramework: null);
        }

        public void RemovePackage(IPackage package)
        {
            if (_packageReferenceFile.DeleteEntry(package.Id, package.Version.ToSemanticVersion()))
            {
                // Remove the repository from the source
                SourceRepository.UnregisterRepository(PackageReferenceFileFullPath);
            }
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            if (!_packageReferenceFile.EntryExists(packageId, version.ToSemanticVersion()))
            {
                return null;
            }

            return SourceRepository.GetPackage(packageId, version);
        }

        public override IQueryable<IPackage> GetPackages(string packageId)
        {
            return GetPackageReferences(packageId).Select(GetPackage).Where(p => p != null).AsQueryable();
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            return _packageReferenceFile.EntryExists(packageId, version.ToSemanticVersion());
        }

        public void RegisterIfNecessary()
        {
            if (GetPackages().Any())
            {
                SourceRepository.RegisterRepository(PackageReferenceFileFullPath);
            }
        }

        public IVersionSpec GetConstraint(string packageId)
        {
            // Find the reference entry for this package
            var reference = GetPackageReference(packageId);
            if (reference != null)
            {
                return reference.VersionConstraint;
            }
            return null;
        }

        //public override bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        //{
        //    PackageReference reference = GetPackageReferences(id).OrderByDescending(r => r.Version)
        //                                                         .FirstOrDefault();
        //    if (reference == null)
        //    {
        //        latestVersion = null;
        //        return false;
        //    }
        //    else
        //    {
        //        latestVersion = reference.Version;
        //        Debug.Assert(latestVersion != null);
        //        return true;
        //    }
        //}

        public override bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package)
        {
            IEnumerable<PackageReference> references = GetPackageReferences(id);
            if (!includePrerelease) 
            {
                references = references.Where(r => String.IsNullOrEmpty(r.Version.SpecialVersion));
            }

            PackageReference reference = references.OrderByDescending(r => r.Version).FirstOrDefault();
            if (reference != null)
            {
                package = GetPackage(reference);
                return true;
            }
            else
            {
                package = null;
                return false;
            }
        }

        public void AddPackage(string packageId, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework)
        {
            _packageReferenceFile.AddEntry(packageId, version.ToSemanticVersion(), developmentDependency, targetFramework);

            // Notify the source repository every time we add a new package to the repository.
            // This doesn't really need to happen on every package add, but this is over agressive
            // to combat scenarios where the 2 repositories get out of sync. If this repository is already 
            // registered in the source then this will be ignored
            SourceRepository.RegisterRepository(PackageReferenceFileFullPath);
        }

        public FrameworkName GetPackageTargetFramework(string packageId)
        {
            var reference = GetPackageReference(packageId);
            if (reference != null)
            {
                return reference.TargetFramework;
            }
            return null;
        }

        private PackageReference GetPackageReference(string packageId)
        {
            return GetPackageReferences(packageId).FirstOrDefault();
        }

        /// <summary>
        /// Gets all references to a specific package id that are valid.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        private IEnumerable<PackageReference> GetPackageReferences(string packageId)
        {
            return _packageReferenceFile.GetPackageReferences()
                                        .Where(reference => IsValidReference(reference) && 
                                                            reference.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        }

        private IPackage GetPackage(PackageReference reference)
        {
            if (IsValidReference(reference))
            {
                return SourceRepository.GetPackage(reference.Id, reference.Version);
            }
            return null;
        }

        private static bool IsValidReference(PackageReference reference)
        {
            return !String.IsNullOrEmpty(reference.Id) && reference.Version != null;
        }
    }
}