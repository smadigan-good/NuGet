using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IMutablePackageRepository : IPackageRepository
    {
        PackageSaveModes PackageSaveMode { get; set; }

        // Which files (nuspec/nupkg) are saved is controlled by property PackageSaveMode.
        void AddPackage(IPackage package);

        void RemovePackage(IPackage package);
    }
}
