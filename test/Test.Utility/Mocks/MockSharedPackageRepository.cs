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
                _references[package.Id] = package.Version.ToSemanticVersion();
            }
            else
            {
                _solutionReferences[package.Id] = package.Version.ToSemanticVersion();
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

        public override bool IsReferenced(string packageId, SemanticVersion version)
        {
            SemanticVersion storedVersion;
            return _references.TryGetValue(packageId, out storedVersion) && storedVersion == version;
        }

        public override bool IsSolutionReferenced(string packageId, SemanticVersion version)
        {
            SemanticVersion storedVersion;
            return _solutionReferences.TryGetValue(packageId, out storedVersion) && storedVersion == version;
        }

        public override void RegisterRepository(string path)
        {
 
        }

        public override void UnregisterRepository(string path)
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
