using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Test.Mocks
{
    public class MockPackageRepository : PackageRepositoryBase, ICollection<IPackage>, IOperationAwareRepository
    {
        private readonly string _source;
        
        public string LastOperation { get; private set; }
        public string LastMainPackageId { get; private set; }
        public string LastMainPackageVersion { get; private set; }

        public MockPackageRepository()
            : this("")
        {
        }

        public MockPackageRepository(string source)
        {
            Packages = new Dictionary<string, List<IPackage>>();
            _source = source;
        }

        public override string Source
        {
            get
            {
                return _source;
            }
        }

        public override bool SupportsPrereleasePackages
        {
            get
            {
                return true;
            }
        }

        internal Dictionary<string, List<IPackage>> Packages
        {
            get;
            set;
        }

        public override void AddPackage(IPackage package)
        {
            AddPackage(package.Id, package);
        }
        
        public override IQueryable<IPackage> GetPackages()
        {
            return Packages.Values.SelectMany(p => p).AsQueryable();
        }

        public override void RemovePackage(IPackage package)
        {
            List<IPackage> packages;
            if (Packages.TryGetValue(package.Id, out packages))
            {
                packages.Remove(package);
            }

            if (packages.Count == 0)
            {
                Packages.Remove(package.Id);
            }
        }

        private void AddPackage(string id, IPackage package)
        {
            List<IPackage> packages;
            if (!Packages.TryGetValue(id, out packages))
            {
                packages = new List<IPackage>();
                Packages.Add(id, packages);
            }
            packages.Add(package);
        }

        public void Add(IPackage item)
        {
            AddPackage(item);
        }

        public void Clear()
        {
            Packages.Clear();
        }

        public bool Contains(IPackage item)
        {
            return this.Exists(item);
        }

        public void CopyTo(IPackage[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get
            {
                return GetPackages().Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(IPackage item)
        {
            if (this.Exists(item))
            {
                RemovePackage(item);
                return true;
            }
            return false;
        }

        public IEnumerator<IPackage> GetEnumerator()
        {
            return GetPackages().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool TryGetLatestPackageVersion(string id, out SemanticVersion latestVersion)
        {
            List<IPackage> packages;
            bool result = Packages.TryGetValue(id, out packages);
            if (result && packages.Count > 0)
            {
                packages.Sort((a, b) => b.Version.CompareTo(a.Version));
                latestVersion = packages[0].Version;
                return true;
            }
            else
            {
                latestVersion = null;
                return false;
            }
        }

        public override bool TryGetLatestPackage(string id, bool includePrerelease, out IPackage package)
        {
            List<IPackage> packages;
            bool result = Packages.TryGetValue(id, out packages);
            if (result && packages.Count > 0)
            {
                // do not modify the actual list
                List<IPackage> workingPackages = new List<IPackage>(packages);

                // remove unlisted packages
                workingPackages.RemoveAll(p => !p.IsListed());

                if (!includePrerelease)
                {
                    workingPackages.RemoveAll(p => !p.IsReleaseVersion());
                }

                if (workingPackages.Count > 0)
                {
                    package = workingPackages.OrderByDescending(p => p.Version).FirstOrDefault();
                    return true;
                }
            }

            package = null;
            return false;
        }

        public override IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            LastOperation = null;
            LastMainPackageId = null;
            LastMainPackageVersion = null;
            return new DisposableAction(() => 
            { 
                LastOperation = operation;
                LastMainPackageId = mainPackageId;
                LastMainPackageVersion = mainPackageVersion;
            });
        }

        public override bool Exists(string packageId, SemanticVersion version)
        {
            return GetPackage(packageId, version) != null;
        }

        public override IPackage GetPackage(string packageId, SemanticVersion version)
        {
            List<IPackage> packages;
            if (Packages.TryGetValue(packageId, out packages))
            {
                return packages.Find(p => version == null || p.Version.Equals(version));
            }
            return null;
        }

        public override IEnumerable<IPackage> GetPackages(string packageId)
        {
            List<IPackage> packages;
            if (Packages.TryGetValue(packageId, out packages))
            {
                return packages;
            }
            return Enumerable.Empty<IPackage>();
        }
    }
}
