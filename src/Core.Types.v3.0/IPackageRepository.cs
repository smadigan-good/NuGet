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
        IQueryable<IPackage> GetPackages();

        /// <summary>
        /// Returns a sequence of packages with the specified id.
        /// </summary>
        IQueryable<IPackage> GetPackages(string packageId);

        IQueryable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted);

        IQueryable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec);

        IQueryable<IPackageName> GetPackageIds();

        IQueryable<IPackageName> GetPackageIds(string packageId);

        IQueryable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted);

        IQueryable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec);

        /// <summary>
        /// Determines if a package exists in a repository.
        /// </summary>
        bool Exists(string packageId, INuGetVersion version);

        bool Exists(string packageId);

        bool Exists(IPackageName package);

        // bool TryGetLatestPackageVersion(string packageId, out SemanticVersion latestVersion);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, out IPackage package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, out IPackage package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec, out IPackage package);

        bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec, IPackageConstraintProvider constraintProvider, out IPackage package);


        bool TryGetPackage(string packageId, INuGetVersion version, out IPackage package);

        bool TryGetPackage(string packageId, INuGetVersion version, bool allowPrereleaseVersions, bool allowUnlisted, out IPackage package);

        bool TryGetPackage(string packageId, INuGetVersion version, bool allowPrereleaseVersions, bool allowUnlisted, IPackageConstraintProvider constraintProvider, out  IPackage package);


        /// <summary>
        /// Finds a package with the exact Id and version. If the repository contains multiple
        /// copies of the same package it should determine a single package to return.
        /// </summary>
        /// <returns>The package if found, null otherwise.</returns>
        IPackage GetPackage(string packageId, INuGetVersion version);

        IPackage GetPackage(string packageId, INuGetVersion version, bool allowPrereleaseVersions, bool allowUnlisted);


        // REMOVE
        // IPackage FindPackage(string packageId, IVersionSpec versionSpec, bool allowPrereleaseVersions, bool allowUnlisted);

        // IPackage FindPackage(string packageId);

        //IPackage GetPackage(string packageId, SemanticVersion version, bool allowPrereleaseVersions, bool allowUnlisted);

        //IPackage FindPackage(string packageId, IVersionSpec versionSpec, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted);

        //IPackage FindPackage(string packageId, SemanticVersion version, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted);

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

        IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions);

        IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks);

        IEnumerable<IPackage> GetCompatiblePackages(IPackageConstraintProvider constraintProvider,
                                                                   IEnumerable<string> packageIds,
                                                                   IPackage package,
                                                                   FrameworkName targetFramework,
                                                                   bool allowPrereleaseVersions);

        // TODO: rework this
        IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion);
    }
}