using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet
{
    [Flags]
    public enum PackageSaveModes
    {
        None = 0, 
        Nuspec = 1,

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming", 
            "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            MessageId = "Nupkg", 
            Justification = "nupkg is the file extension of the package file")]
        Nupkg = 2        
    }

    public interface IPackageRepository : ICloneable
    {
        string Source { get; }

        PackageSaveModes PackageSaveMode { get; set; }

        // Remove this property?
        bool SupportsPrereleasePackages { get; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This call might be expensive")]
        IQueryable<IPackage> GetPackages();

        // Which files (nuspec/nupkg) are saved is controlled by property PackageSaveMode.
        void AddPackage(IPackage package);

        void RemovePackage(IPackage package);

        /// <summary>
        /// Determines if a package exists in a repository.
        /// </summary>
        bool Exists(string packageId, SemanticVersion version);

        bool Exists(string packageId);

        bool Exists(IPackageName package);

        bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion);

        bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package);

        bool TryGetLatestPackage(string id, bool includePrerelease, bool includeUnlisted, out IPackage package);

        IPackage GetAbsoluteLatestPackage(string id);

        bool TryGetPackage(string packageId, SemanticVersion version, out IPackage package);

        /// <summary>
        /// Returns a sequence of packages with the specified id.
        /// </summary>
        IEnumerable<IPackage> GetPackages(string packageId);


        /// <summary>
        /// Finds a package with the exact Id and version. If the repository contains multiple
        /// copies of the same package it should determine a single package to return.
        /// </summary>
        /// <returns>The package if found, null otherwise.</returns>
        IPackage GetPackage(string packageId, SemanticVersion version);

        IEnumerable<IPackage> GetPackages(string packageId, IVersionSpec versionSpec, bool allowPrereleaseVersions, bool allowUnlisted);

        // REMOVE
        IPackage FindPackage(string packageId, IVersionSpec versionSpec, bool allowPrereleaseVersions, bool allowUnlisted);

        IEnumerable<IPackage> FindPackages(IEnumerable<string> packageIds);

        // IPackage FindPackage(string packageId);

        IPackage FindPackage(string packageId, SemanticVersion version, bool allowPrereleaseVersions, bool allowUnlisted);

        IPackage FindPackage(string packageId, IVersionSpec versionSpec, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted);

        IPackage FindPackage(string packageId, SemanticVersion version, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted);





        IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFrameworks,
            IEnumerable<IVersionSpec> versionConstraints);

        IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFrameworks);

        IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions);

        IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion);
        IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages);
        IPackage ResolveDependency(PackageDependency dependency, bool allowPrereleaseVersions, bool preferListedPackages);

        IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions);

        IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions);

        IEnumerable<IPackage> FindCompatiblePackages(IPackageConstraintProvider constraintProvider,
                                                                   IEnumerable<string> packageIds,
                                                                   IPackage package,
                                                                   FrameworkName targetFramework,
                                                                   bool allowPrereleaseVersions);

        IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion);

        IEnumerable<IPackage> GetUpdatesCore(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFramework,
            IEnumerable<IVersionSpec> versionConstraints);
    }
}