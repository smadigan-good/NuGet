using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet
{
    public interface IPackageRepository : ICloneable
    {
        string Source { get; }

        CultureInfo Culture { get; }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This call might be expensive")]
        IEnumerable<IPackage> GetPackages();

        /// <summary>
        /// Returns a sequence of packages with the specified id.
        /// </summary>
        IEnumerable<IPackage> GetPackages(string packageId);

        IEnumerable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted);

        IEnumerable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec);


        // All metadata search
        IEnumerable<IPackageMetadata> GetPackageMetadata();

        IEnumerable<IPackageMetadata> GetPackageMetadata(string packageId, bool allowPrereleaseVersions, bool allowUnlisted);

        IEnumerable<IPackageMetadata> GetPackageMetadata(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec);

        // Weak name search
        IEnumerable<IPackageName> GetPackageIds();

        IEnumerable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted);

        IEnumerable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec);


        /// <summary>
        /// Determines if a package exists in a repository.
        /// </summary>
        bool Exists(string packageId, INuGetVersion version);

        bool Exists(string packageId);

        bool Exists(IPackageName package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, out IPackage package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, out IPackage package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec, out IPackage package);

        bool TryGetPackage(string packageId, INuGetVersion version, out IPackage package);

        //bool TryGetIRIPackage(string packageIRI, out IPackage package);

        /// <summary>
        /// Finds a package with the exact Id and version. If the repository contains multiple
        /// copies of the same package it should determine a single package to return.
        /// </summary>
        /// <returns>The package if found, null otherwise.</returns>
        IPackage GetPackage(string packageId, INuGetVersion version);

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

        IPackage ResolveDependency(IPackageDependency dependency, DependencyVersion dependencyVersion, bool allowPrereleaseVersions, bool preferListedPackages);

        IPackage ResolveDependency(IPackageDependency dependency, DependencyVersion dependencyVersion, bool allowPrereleaseVersions, bool preferListedPackages, IPackageConstraintProvider constraintProvider);

        IEnumerable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions);

        IEnumerable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks);

        IEnumerable<IPackage> GetCompatiblePackages(IPackageConstraintProvider constraintProvider,
                                                                   IEnumerable<string> packageIds,
                                                                   IPackage package,
                                                                   FrameworkName targetFramework,
                                                                   bool allowPrereleaseVersions);

        IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion);

        PackageSaveModes PackageSaveMode { get; set; }

        // Which files (nuspec/nupkg) are saved is controlled by property PackageSaveMode.
        void AddPackage(IPackage package);

        void RemovePackage(IPackage package);
    }
}