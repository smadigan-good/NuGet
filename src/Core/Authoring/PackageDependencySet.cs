using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet
{
    public class PackageDependencySet : IPackageDependencySet
    {
        private readonly FrameworkName _targetFramework;
        private readonly ReadOnlyCollection<IPackageDependency> _dependencies;

        public PackageDependencySet(FrameworkName targetFramework, IEnumerable<IPackageDependency> dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }

            _targetFramework = targetFramework;
            _dependencies = new ReadOnlyCollection<IPackageDependency>(dependencies.ToList());
        }

        public FrameworkName TargetFramework
        {
            get
            {
                return _targetFramework;
            }
        }

        public IEnumerable<IPackageDependency> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }

        public IEnumerable<FrameworkName> SupportedFrameworks
        {
            get 
            {
                if (TargetFramework == null)
                {
                    yield break;
                }

                yield return TargetFramework;
            }
        }
    }
}