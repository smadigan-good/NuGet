using NuGet.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Versioning;

namespace NuGet
{
    public abstract class PackageRepositoryBase : IPackageRepository
    {
        public abstract string Source { get; }

        /*
        public abstract IQueryable<IPackage> GetPackages();

        public virtual void AddPackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        public virtual void RemovePackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        public virtual bool Exists(string packageId, SemanticVersion version)
        {
            return GetPackage(packageId, version) != null;
        }

        public virtual IPackage GetPackage(string packageId, SemanticVersion version)
        {
            return GetPackages(packageId).Where(p => p.Version == version).FirstOrDefault();
        }

        public virtual IEnumerable<IPackage> GetPackages(string packageId)
        {
            var cultureRepository = this as ICultureAwareRepository;
            if (cultureRepository != null)
            {
                packageId = packageId.ToLower(cultureRepository.Culture);
            }
            else
            {
                packageId = packageId.ToLower(CultureInfo.CurrentCulture);
            }

            return (from p in GetPackages()
                    where p.Id.ToLower() == packageId
                    orderby p.Id
                    select p).ToList();
        }

        //-000000000000000000000000000000
        public virtual IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            IOperationAwareRepository repo = this as IOperationAwareRepository;
            if (repo != null)
            {
                return repo.StartOperation(operation, mainPackageId, mainPackageVersion);
            }
            return DisposableAction.NoOp;
        }

        public virtual bool Exists(IPackageName package)
        {
            return Exists(package.Id, package.Version);
        }


        public virtual bool Exists(string packageId)
        {
            // return Exists(repository, packageId, version: null);

            return !GetPackages(packageId).IsEmpty();
        }

        public virtual bool TryGetPackage(string packageId, SemanticVersion version, out IPackage package)
        {
            package = GetPackage(packageId, version);
            return package != null;
        }

        public virtual IPackage FindPackage(string packageId)
        {
            // TODO: Rewrite this
            return GetPackages(packageId).FirstOrDefault();
        }

        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            // Default allow pre release versions to true here because the caller typically wants to find all packages in this scenario for e.g when checking if a 
            // a package is already installed in the local repository. The same applies to allowUnlisted.
            return FindPackage(repository, packageId, version, NullConstraintProvider.Instance, allowPrereleaseVersions: true, allowUnlisted: true);
        } 

        public virtual IPackage GetPackage(string packageId, SemanticVersion version, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            return FindPackage(packageId, version, NullConstraintProvider.Instance, allowPrereleaseVersions, allowUnlisted);
        }

        public virtual IPackage FindPackage(
            string packageId,
            SemanticVersion version,
            IPackageConstraintProvider constraintProvider,
            bool allowPrereleaseVersions,
            bool allowUnlisted)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }

            // if an explicit version is specified, disregard the 'allowUnlisted' argument
            // and always allow unlisted packages.
            if (version != null)
            {
                allowUnlisted = true;
            }
            else if (!allowUnlisted && (constraintProvider == null || constraintProvider == NullConstraintProvider.Instance))
            {
                // this is now always a ILatestPackageLookup
                IPackage package;
                if (TryGetLatestPackage(packageId, allowPrereleaseVersions, allowUnlisted, out package))
                {
                    return package;
                }
            }

            // If the repository implements it's own lookup then use that instead.
            // This is an optimization that we use so we don't have to enumerate packages for
            // sources that don't need to.
            if (version != null)
            {
                return GetPackage(packageId, version);
            }

            IEnumerable<IPackage> packages = GetPackages(packageId);

            packages = packages.ToList()
                               .OrderByDescending(p => p.Version);

            if (!allowUnlisted)
            {
                packages = packages.Where(PackageExtensions.IsListed);
            }

            if (version != null)
            {
                packages = packages.Where(p => p.Version == version);
            }
            else if (constraintProvider != null)
            {
                packages = FilterPackagesByConstraints(constraintProvider, packages, packageId, allowPrereleaseVersions);
            }

            return packages.FirstOrDefault();
        }

        public virtual IPackage FindPackage(string packageId, IVersionSpec versionSpec,
                IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            var packages = GetPackages(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);

            if (constraintProvider != null)
            {
                packages = FilterPackagesByConstraints(constraintProvider, packages, packageId, allowPrereleaseVersions);
            }

            return packages.FirstOrDefault();
        }

        public virtual IEnumerable<IPackage> GetPackages(IEnumerable<string> packageIds)
        {
            if (packageIds == null)
            {
                throw new ArgumentNullException("packageIds");
            }

            return FindPackages(packageIds, GetFilterExpression);
        }

        public virtual IEnumerable<IPackage> GetPackages(
            string packageId,
            IVersionSpec versionSpec,
            bool allowPrereleaseVersions,
            bool allowUnlisted)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }

            IEnumerable<IPackage> packages = GetPackages(packageId)
                                                       .OrderByDescending(p => p.Version);

            if (!allowUnlisted)
            {
                packages = packages.Where(PackageExtensions.IsListed);
            }

            if (versionSpec != null)
            {
                packages = packages.FindByVersion(versionSpec);
            }

            packages = FilterPackagesByConstraints(NullConstraintProvider.Instance, packages, packageId, allowPrereleaseVersions);

            return packages;
        }

        public virtual IPackage FindPackage(
            string packageId,
            IVersionSpec versionSpec,
            bool allowPrereleaseVersions,
            bool allowUnlisted)
        {
            return GetPackages(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted).FirstOrDefault();
        }

        public virtual IEnumerable<IPackage> GetCompatiblePackages(IPackageConstraintProvider constraintProvider,
                                                                   IEnumerable<string> packageIds,
                                                                   IPackage package,
                                                                   FrameworkName targetFramework,
                                                                   bool allowPrereleaseVersions)
        {
            return (from p in GetPackages(packageIds)
                    where allowPrereleaseVersions || p.IsReleaseVersion()
                    let dependency = p.FindDependency(package.Id, targetFramework)
                    let otherConstaint = constraintProvider.GetConstraint(p.Id)
                    where dependency != null &&
                          dependency.VersionSpec.Satisfies(package.Version) &&
                          (otherConstaint == null || otherConstaint.Satisfies(package.Version))
                    select p);
        }

        public virtual IQueryable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions)
        {
            return Search(searchTerm, targetFrameworks: Enumerable.Empty<string>(), allowPrereleaseVersions: allowPrereleaseVersions);
        }

        public virtual IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions)
        {
            if (targetFrameworks == null)
            {
                throw new ArgumentNullException("targetFrameworks");
            }

            var serviceBasedRepository = this as IServiceBasedRepository;
            if (serviceBasedRepository != null)
            {
                return serviceBasedRepository.Search(searchTerm, targetFrameworks, allowPrereleaseVersions);
            }

            // Ignore the target framework if the repository doesn't support searching
            return GetPackages().Find(searchTerm)
                                           .FilterByPrerelease(allowPrereleaseVersions)
                                           .AsQueryable();
        }

        public virtual IPackage ResolveDependency(PackageDependency dependency, bool allowPrereleaseVersions, bool preferListedPackages)
        {
            //return ResolveDependency(dependency, constraintProvider: null, allowPrereleaseVersions: allowPrereleaseVersions, preferListedPackages: preferListedPackages, dependencyVersion: DependencyVersion.Lowest);

            throw new NotImplementedException();
        }

        public virtual IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages)
        {
            return ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion: DependencyVersion.Lowest);
        }

        public virtual IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            IDependencyResolver dependencyResolver = this as IDependencyResolver;
            if (dependencyResolver != null)
            {
                return dependencyResolver.ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion);
            }
            return ResolveDependencyCore(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion);
        }

        internal IPackage ResolveDependencyCore(
            PackageDependency dependency,
            IPackageConstraintProvider constraintProvider,
            bool allowPrereleaseVersions,
            bool preferListedPackages,
            DependencyVersion dependencyVersion)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }

            IEnumerable<IPackage> packages = GetPackages(dependency.Id).ToList();

            // Always filter by constraints when looking for dependencies
            packages = FilterPackagesByConstraints(constraintProvider, packages, dependency.Id, allowPrereleaseVersions);

            IList<IPackage> candidates = packages.ToList();

            if (preferListedPackages)
            {
                // pick among Listed packages first
                IPackage listedSelectedPackage = ResolveDependencyCore(
                    candidates.Where(PackageExtensions.IsListed),
                    dependency,
                    dependencyVersion);
                if (listedSelectedPackage != null)
                {
                    return listedSelectedPackage;
                }
            }

            return ResolveDependencyCore(candidates, dependency, dependencyVersion);
        }

        /// <summary>
        /// From the list of packages <paramref name="packages"/>, selects the package that best 
        /// matches the <paramref name="dependency"/>.
        /// </summary>
        /// <param name="packages">The list of packages.</param>
        /// <param name="dependency">The dependency used to select package from the list.</param>
        /// <param name="dependencyVersion">Indicates the method used to select dependency. 
        /// Applicable only when dependency.VersionSpec is not null.</param>
        /// <returns>The selected package.</returns>
        private static IPackage ResolveDependencyCore(
            IEnumerable<IPackage> packages,
            PackageDependency dependency,
            DependencyVersion dependencyVersion)
        {
            // If version info was specified then use it
            if (dependency.VersionSpec != null)
            {
                packages = packages.FindByVersion(dependency.VersionSpec).OrderBy(p => p.Version);
                return packages.SelectDependency(dependencyVersion);
            }
            else
            {
                // BUG 840: If no version info was specified then pick the latest
                return packages.OrderByDescending(p => p.Version)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns updates for packages from the repository 
        /// </summary>
        /// <param name="packages">Packages to look for updates</param>
        /// <param name="includePrerelease">Indicates whether to consider prerelease updates.</param>
        /// <param name="includeAllVersions">Indicates whether to include all versions of an update as opposed to only including the latest version.</param>
        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions)
        {
            return GetUpdates(packages, includePrerelease, includeAllVersions, null, null);
        }

        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFrameworks)
        {
            return GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, null);
        }


        /// <summary>
        /// Returns updates for packages from the repository 
        /// </summary>
        /// <param name="packages">Packages to look for updates</param>
        /// <param name="includePrerelease">Indicates whether to consider prerelease updates.</param>
        /// <param name="includeAllVersions">Indicates whether to include all versions of an update as opposed to only including the latest version.</param>
        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFrameworks,
            IEnumerable<IVersionSpec> versionConstraints)
        {
            if (packages.IsEmpty())
            {
                return Enumerable.Empty<IPackage>();
            }

            var serviceBasedRepository = this as IServiceBasedRepository;
            return serviceBasedRepository != null ? serviceBasedRepository.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints) :
                                                    GetUpdatesCore(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
        }

        public virtual IEnumerable<IPackage> GetUpdatesCore(IEnumerable<IPackageName> packages,
            bool includePrerelease,
            bool includeAllVersions,
            IEnumerable<FrameworkName> targetFramework,
            IEnumerable<IVersionSpec> versionConstraints)
        {
            List<IPackageName> packageList = packages.ToList();

            if (!packageList.Any())
            {
                return Enumerable.Empty<IPackage>();
            }

            IList<IVersionSpec> versionConstraintList;
            if (versionConstraints == null)
            {
                versionConstraintList = new IVersionSpec[packageList.Count];
            }
            else
            {
                versionConstraintList = versionConstraints.ToList();
            }

            if (packageList.Count != versionConstraintList.Count)
            {
                throw new ArgumentException(NuGetResources.GetUpdatesParameterMismatch);
            }

            // These are the packages that we need to look at for potential updates.
            ILookup<string, IPackage> sourcePackages = GetUpdateCandidates(packageList, includePrerelease)
                                                                            .ToList()
                                                                            .ToLookup(package => package.Id, StringComparer.OrdinalIgnoreCase);

            var results = new List<IPackage>();
            for (int i = 0; i < packageList.Count; i++)
            {
                var package = packageList[i];
                var constraint = versionConstraintList[i];

                var updates = from candidate in sourcePackages[package.Id]
                              where (candidate.Version > package.Version) &&
                                     SupportsTargetFrameworks(targetFramework, candidate) &&
                                     (constraint == null || constraint.Satisfies(candidate.Version))
                              select candidate;

                results.AddRange(updates);
            }

            if (!includeAllVersions)
            {
                return results.CollapseById();
            }
            return results;
        }

        public virtual object Clone()
        {
            // Do nothing special
            return this;
        }

        /// <summary>
        /// Since odata dies when our query for updates is too big. We query for updates 10 packages at a time
        /// and return the full list of candidates for updates.
        /// </summary>
        private IEnumerable<IPackage> GetUpdateCandidates(
            IEnumerable<IPackageName> packages,
            bool includePrerelease)
        {

            var query = FindPackages(packages, GetFilterExpression);
            if (!includePrerelease)
            {
                query = query.Where(p => p.IsReleaseVersion());
            }

            // for updates, we never consider unlisted packages
            query = query.Where(PackageExtensions.IsListed);

            return query;
        }

        private static bool SupportsTargetFrameworks(IEnumerable<FrameworkName> targetFramework, IPackage package)
        {
            return targetFramework.IsEmpty() || targetFramework.Any(t => VersionUtility.IsCompatible(t, package.GetSupportedFrameworks()));
        }

        /// <summary>
        /// For the list of input packages generate an expression like:
        /// p => p.Id == 'package1id' or p.Id == 'package2id' or p.Id == 'package3id'... up to package n
        /// </summary>
        private static Expression<Func<IPackage, bool>> GetFilterExpression(IEnumerable<IPackageName> packages)
        {
            return GetFilterExpression(packages.Select(p => p.Id));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower", Justification = "This is for a linq query")]
        private static Expression<Func<IPackage, bool>> GetFilterExpression(IEnumerable<string> ids)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IPackageName));
            Expression expressionBody = ids.Select(id => GetCompareExpression(parameterExpression, id.ToLower()))
                                                .Aggregate(Expression.OrElse);

            return Expression.Lambda<Func<IPackage, bool>>(expressionBody, parameterExpression);
        }

        /// <summary>
        /// Builds the expression: package.Id.ToLower() == "somepackageid"
        /// </summary>
        private static Expression GetCompareExpression(Expression parameterExpression, object value)
        {
            // package.Id
            Expression propertyExpression = Expression.Property(parameterExpression, "Id");
            // .ToLower()
            Expression toLowerExpression = Expression.Call(propertyExpression, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
            // == localPackage.Id
            return Expression.Equal(toLowerExpression, Expression.Constant(value));
        }

        private static IEnumerable<IPackage> FilterPackagesByConstraints(
            IPackageConstraintProvider constraintProvider,
            IEnumerable<IPackage> packages,
            string packageId,
            bool allowPrereleaseVersions)
        {
            constraintProvider = constraintProvider ?? NullConstraintProvider.Instance;

            // Filter packages by this constraint
            IVersionSpec constraint = constraintProvider.GetConstraint(packageId);
            if (constraint != null)
            {
                packages = packages.FindByVersion(constraint);
            }
            if (!allowPrereleaseVersions)
            {
                packages = packages.Where(p => p.IsReleaseVersion());
            }

            return packages;
        }

        /// <summary>
        /// Since Odata dies when our query for updates is too big. We query for updates 10 packages at a time
        /// and return the full list of packages.
        /// </summary>
        private IEnumerable<IPackage> FindPackages<T>(IEnumerable<T> items, Func<IEnumerable<T>, Expression<Func<IPackage, bool>>> filterSelector)
        {
            const int batchSize = 10;

            while (items.Any())
            {
                IEnumerable<T> currentItems = items.Take(batchSize);
                Expression<Func<IPackage, bool>> filterExpression = filterSelector(currentItems);

                var query = GetPackages()
                                      .Where(filterExpression)
                                      .OrderBy(p => p.Id);

                foreach (var package in query)
                {
                    yield return package;
                }

                items = items.Skip(batchSize);
            }
        }

        public virtual bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        {
            latestVersion = null;
            IPackage package = null;
            if (TryGetLatestPackage(id, false, out package))
            {
                latestVersion = package.Version;
            }

            return latestVersion != null;
        }

        public virtual bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package)
        {
            return TryGetLatestPackage(id, includePrerelease, true, out package);
        }

        public virtual bool TryGetLatestPackage(string id, bool includePrerelease, bool includeUnlisted, out IPackage package)
        {
            package = GetPackages(id).Where(p => includePrerelease || p.IsReleaseVersion()).Where(p => includeUnlisted || p.IsListed()).OrderByDescending(p => p.Version).FirstOrDefault();

            return package != null;
        }

        */

        public abstract IEnumerable<IPackage> GetPackages();

        public virtual IEnumerable<IPackage> GetPackages(string packageId)
        {
            return GetPackages().Where(p => SameIds(p.Id, packageId));
        }

        public virtual IEnumerable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            return GetPackages(packageId).Where(p => (allowPrereleaseVersions || p.IsReleaseVersion()) && (allowUnlisted || p.IsListed()));
        }

        public virtual IEnumerable<IPackage> GetPackages(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }

            return GetPackages(packageId, allowPrereleaseVersions, allowUnlisted).Where(p => versionSpec.Satisfies(p.Version));
        }

        public virtual bool Exists(string packageId, INuGetVersion version)
        {
            return !GetPackageIds(packageId, true, true).Where(p => VersionComparer.Equals(p.Version, version)).IsEmpty();
        }

        public virtual bool Exists(string packageId)
        {
            return !GetPackageIds(packageId, true, true).IsEmpty();
        }

        public virtual bool Exists(IPackageName package)
        {
            return Exists(package.Id, package.Version);
        }

        public virtual bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, out IPackage package)
        {
            package = GetPackages(packageId, allowPrereleaseVersions, true).OrderByDescending(p => p.Version, VersionComparer).FirstOrDefault();
            return package != null;
        }

        public virtual bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, out IPackage package)
        {
            package = GetPackages(packageId, allowPrereleaseVersions, allowUnlisted).OrderByDescending(p => p.Version, VersionComparer).FirstOrDefault();
            return package != null;
        }

        public virtual bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec, out IPackage package)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }

            package = GetPackages(packageId, allowPrereleaseVersions, allowUnlisted).OrderByDescending(p => p.Version, VersionComparer)
                .Where(p => versionSpec.Satisfies(p.Version)).FirstOrDefault();

            return package != null;
        }

        public virtual bool TryGetLatestPackage(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec, IPackageConstraintProvider constraintProvider, out IPackage package)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }

            package = GetPackages(packageId, allowPrereleaseVersions, allowUnlisted).OrderByDescending(p => p.Version, VersionComparer)
                .Where(p => versionSpec.Satisfies(p.Version)).FirstOrDefault();

            return package != null;
        }

        public virtual bool TryGetPackage(string packageId, INuGetVersion version, out IPackage package)
        {
            return TryGetPackage(packageId, version, true, true, out package);
        }

        public virtual bool TryGetPackage(string packageId, INuGetVersion version, bool allowPrereleaseVersions, bool allowUnlisted, out IPackage package)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            package = GetPackages(packageId)
                .Where(p => allowPrereleaseVersions || !p.Version.IsPrerelease)
                .Where(p => allowUnlisted || p.Listed)
                .Where(p => VersionComparer.Equals(p.Version, version)).FirstOrDefault();

            return package != null;
        }

        public virtual IPackage GetPackage(string packageId, INuGetVersion version)
        {
            IPackage package = null;
            TryGetPackage(packageId, version, out package);
            return package;
        }

        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks, IEnumerable<IVersionSpec> versionConstraints)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions)
        {
            throw new NotImplementedException();
        }

        public virtual IPackage ResolveDependency(IPackageDependency dependency, DependencyVersion dependencyVersion, bool allowPrereleaseVersions, bool preferListedPackages)
        {
            throw new NotImplementedException();
        }

        public virtual IPackage ResolveDependency(IPackageDependency dependency, DependencyVersion dependencyVersion, bool allowPrereleaseVersions, bool preferListedPackages, IPackageConstraintProvider constraintProvider)
        {
            // TODO: How does this know what the major is to get the highest minor?

            //var possiblePackages = GetPackageIds(dependency.Id, allowPrereleaseVersions, true, constraintProvider.GetConstraint(dependency.Id));

            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPackage> Search(string searchTerm, bool allowPrereleaseVersions, IEnumerable<string> targetFrameworks)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IPackage> GetCompatiblePackages(IPackageConstraintProvider constraintProvider, IEnumerable<string> packageIds, IPackage package, FrameworkName targetFramework, bool allowPrereleaseVersions)
        {
            throw new NotImplementedException();
        }

        public virtual IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            throw new NotImplementedException();
        }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        public virtual CultureInfo Culture
        {
            get
            {
                return CultureInfo.InvariantCulture;
            }
        }

        protected IVersionComparer VersionComparer
        {
            get
            {
                return NuGet.Versioning.VersionComparer.Default;
            }
        }


        public virtual PackageSaveModes PackageSaveMode
        {
            get
            {
                return PackageSaveModes.Nupkg;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual void AddPackage(IPackage package)
        {
            throw new NotImplementedException();
        }

        public virtual void RemovePackage(IPackage package)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<IPackageMetadata> GetPackageMetadata()
        {
            return GetPackages().Select(p => p as IPackageMetadata);
        }

        public IEnumerable<IPackageMetadata> GetPackageMetadata(string packageId, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            return GetPackages().Where(p => SameIds(p.Id, packageId))
                .Where(p => allowPrereleaseVersions || p.Version.IsPrerelease)
                .Where(p => allowUnlisted || p.Listed)
                .Select(p => p as IPackageMetadata);
        }

        public IEnumerable<IPackageMetadata> GetPackageMetadata(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }

            return GetPackageMetadata(packageId, allowPrereleaseVersions, allowUnlisted).Where(p => versionSpec.Satisfies(p.Version));
        }

        public IEnumerable<IPackageName> GetPackageIds()
        {
            return GetPackages().Select(p => p as IPackageName);
        }

        public IEnumerable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            return GetPackages().Where(p => SameIds(p.Id, packageId))
                .Where(p => allowPrereleaseVersions || p.Version.IsPrerelease)
                .Where(p => allowUnlisted || p.Listed)
                .Select(p => p as IPackageName);
        }

        public IEnumerable<IPackageName> GetPackageIds(string packageId, bool allowPrereleaseVersions, bool allowUnlisted, IVersionSpec versionSpec)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }

            return GetPackageIds(packageId, allowPrereleaseVersions, allowUnlisted).Where(p => versionSpec.Satisfies(p.Version));
        }

        protected bool SameIds(string x, string y)
        {
            return Culture.CompareInfo.Compare(x, y, CompareOptions.IgnoreCase) == 0;
        }
    }
}
