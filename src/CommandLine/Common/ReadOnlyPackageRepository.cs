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

        public override IEnumerable<IPackage> GetPackages()
        {
            return _packages.AsQueryable();
        }
    }
}
