using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NuGet.Test.Mocks
{
    public class MockSharedPackageRepository : MockPackageRepository, ISharedPackageRepository
    {
        private Dictionary<string, SemanticVersion> _references = 
            new Dictionary<string, SemanticVersion>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, SemanticVersion> _solutionReferences = 
            new Dictionary<string, SemanticVersion>(StringComparer.OrdinalIgnoreCase);

        public MockSharedPackageRepository()
            : this("")
        {
        }

        public MockSharedPackageRepository(string source) : base(source)
        {
        }

        public void ClearReferences()
        {
            _references.Clear();
        }

        public void AddReference(string packageId, SemanticVersion version)
        {
            _references.Add(packageId, version);
        }

        public override void AddPackage(IPackage package)
        {
            base.AddPackage(package);

            if (package.HasProjectContent())
            {
                _references[package.Id] = package.Version;
            }
            else
            {
                _solutionReferences[package.Id] = package.Version;
            }
        }

        public override void RemovePackage(IPackage package)
        {
            base.RemovePackage(package);

            var otherPackages = GetPackages(package.Id).Where(p => p != package);

            if (otherPackages.IsEmpty())
            {
                if (package.HasProjectContent())
                {
                    _references.Remove(package.Id);
                }
                else
                {
                    _solutionReferences.Remove(package.Id);
                }
            }
        }
        
        public bool IsReferenced(string packageId, SemanticVersion version)
        {
            SemanticVersion storedVersion;
            return _references.TryGetValue(packageId, out storedVersion) && storedVersion == version;
        }

        public bool IsSolutionReferenced(string packageId, SemanticVersion version)
        {
            SemanticVersion storedVersion;
            return _solutionReferences.TryGetValue(packageId, out storedVersion) && storedVersion == version;
        }

        public void RegisterRepository(string path)
        {
 
        }

        public void UnregisterRepository(string path)
        {

        }

        // TODO: Remove this
        public MockSharedPackageRepository Object
        {
            get
            {
                return this;
            }
        }
    }
}
