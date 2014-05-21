using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Versioning;
using NuGet.Resources;

namespace NuGet
{
    public static class PackageRepositoryExtensions
    {
        public static PackageDependency FindDependency(this IPackageMetadata package, string packageId, FrameworkName targetFramework)
        {
            return (from dependency in package.GetCompatiblePackageDependencies(targetFramework)
                    where dependency.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase)
                    select dependency).FirstOrDefault();
        }

        /// <summary>
        /// Selects the dependency package from the list of candidate packages 
        /// according to <paramref name="dependencyVersion"/>.
        /// </summary>
        /// <param name="packages">The list of candidate packages.</param>
        /// <param name="dependencyVersion">The rule used to select the package from 
        /// <paramref name="packages"/> </param>
        /// <returns>The selected package.</returns>
        /// <remarks>Precondition: <paramref name="packages"/> are ordered by ascending version.</remarks>        
        internal static IPackage SelectDependency(this IEnumerable<IPackage> packages, DependencyVersion dependencyVersion)
        {
            if (packages == null || !packages.Any())
            {
                return null;
            }

            if (dependencyVersion == DependencyVersion.Lowest)
            {
                return packages.FirstOrDefault();
            }
            else if (dependencyVersion == DependencyVersion.Highest)
            {
                return packages.LastOrDefault();
            }
            else if (dependencyVersion == DependencyVersion.HighestPatch)
            {
                var groups = from p in packages
                             group p by new { p.Version.Version.Major, p.Version.Version.Minor } into g
                             orderby g.Key.Major, g.Key.Minor
                             select g;
                return (from p in groups.First()
                        orderby p.Version descending
                        select p).FirstOrDefault();
            }
            else if (dependencyVersion == DependencyVersion.HighestMinor)
            {
                var groups = from p in packages
                             group p by new { p.Version.Version.Major } into g
                             orderby g.Key.Major
                             select g;
                return (from p in groups.First()
                        orderby p.Version descending
                        select p).FirstOrDefault();
            }            

            throw new ArgumentOutOfRangeException("dependencyVersion");
        }
    }
}