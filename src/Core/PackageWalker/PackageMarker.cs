using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet
{
    /// <summary>
    /// Keeps track of a package's visited state while walking a graph. It also acts as a package repository and
    /// a dependents resolver for the live graph.
    /// </summary>
    public sealed class PackageMarker : PackageRepositoryBase, IDependentsResolver
    {
        private readonly Dictionary<string, Dictionary<IPackage, VisitedState>> _visited = new Dictionary<string, Dictionary<IPackage, VisitedState>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<IPackage, HashSet<IPackage>> _dependents = new Dictionary<IPackage, HashSet<IPackage>>(PackageEqualityComparer.IdAndVersion);

        public override string Source
        {
            get
            {
                // source doesn't matter for this class
                return String.Empty;
            }
        }

        // PackageSaveMode property does not apply to this class
        public override PackageSaveModes PackageSaveMode
        {
            set { throw new NotSupportedException(); }
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns all packages regardless we've ever seen
        /// </summary>
        public IEnumerable<IPackage> Packages
        {
            get
            {
                return _visited.Values.SelectMany(p => p.Keys);
            }
        }

        public bool Contains(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(package.Id, createEntry: true);
            return lookup != null && lookup.ContainsKey(package);
        }

        public void MarkProcessing(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(package.Id, createEntry: true);
            lookup[package] = VisitedState.Processing;
        }

        public void MarkVisited(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(package.Id, createEntry: true);
            lookup[package] = VisitedState.Completed;
        }

        public bool IsVersionCycle(string packageId)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(packageId);
            return lookup != null && lookup.Values.Any(state => state == VisitedState.Processing);
        }

        public bool IsVisited(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(package.Id);
            VisitedState state;
            return lookup != null && lookup.TryGetValue(package, out state) && state == VisitedState.Completed;
        }

        public bool IsCycle(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = GetLookup(package.Id);
            VisitedState state;
            return lookup != null && lookup.TryGetValue(package, out state) && state == VisitedState.Processing;
        }

        public void Clear()
        {
            _visited.Clear();
            _dependents.Clear();
        }

        public override IEnumerable<IPackage> GetPackages()
        {
            // Return visited packages only
            return Packages.Where(IsVisited).AsQueryable();
        }

        IEnumerable<IPackage> IDependentsResolver.GetDependents(IPackage package)
        {
            HashSet<IPackage> dependents;
            if (_dependents.TryGetValue(package, out dependents))
            {
                return dependents;
            }
            return Enumerable.Empty<IPackage>();
        }

        /// <summary>
        /// While walking the package graph we call this to update dependents.
        /// </summary>
        public void AddDependent(IPackage package, IPackage dependency)
        {
            HashSet<IPackage> values;
            if (!_dependents.TryGetValue(dependency, out values))
            {
                values = new HashSet<IPackage>(PackageEqualityComparer.IdAndVersion);
                _dependents.Add(dependency, values);
            }

            // Add the current package to the list of dependents
            values.Add(package);
        }

        private Dictionary<IPackage, VisitedState> GetLookup(string packageId, bool createEntry = false)
        {
            Dictionary<IPackage, VisitedState> state;
            if (!_visited.TryGetValue(packageId, out state))
            {
                if (createEntry)
                {
                    state = new Dictionary<IPackage, VisitedState>(PackageEqualityComparer.IdAndVersion);
                    _visited[packageId] = state;
                }
            }
            return state;
        }

        internal enum VisitedState
        {
            Processing,
            Completed
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            return GetPackage(packageId, version) != null;
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            return GetPackages(packageId).Where(p => p.Version.Equals(version)).FirstOrDefault();
        }

        public override IEnumerable<IPackage> GetPackages(string packageId)
        {
            Dictionary<IPackage, VisitedState> packages = GetLookup(packageId);
            if (packages != null)
            {
                return packages.Keys.Where(p => packages[p] == VisitedState.Completed).AsQueryable();
            }

            return Enumerable.Empty<IPackage>().AsQueryable();
        }
    }
}
