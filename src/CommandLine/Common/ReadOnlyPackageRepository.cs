﻿using System.Collections.Generic;
using System.Linq;

namespace NuGet.Common
{
    public class ReadOnlyPackageRepository : PackageRepositoryBase
    {
        private readonly IEnumerable<IPackage> _packages;
        public ReadOnlyPackageRepository(IEnumerable<IPackage> packages)
        {
            _packages = packages;
        }

        public override string Source
        {
            get { return null; }
        }

        public override IQueryable<IPackage> GetPackages()
        {
            return _packages.AsQueryable();
        }

        public override void AddPackage(IPackage package)
        {
            throw new System.NotImplementedException();
        }

        public override void RemovePackage(IPackage package)
        {
            throw new System.NotImplementedException();
        }
    }
}
