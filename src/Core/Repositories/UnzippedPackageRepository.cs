using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuGet
{
    public class UnzippedPackageRepository : FileSystemRepository
    {
        public UnzippedPackageRepository(string physicalPath)
            : this(new DefaultPackagePathResolver(physicalPath), new PhysicalFileSystem(physicalPath))
        {

        }

        public UnzippedPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : base(pathResolver, fileSystem)
        {

        }

        public override string Source
        {
            get { return FileSystem.Root; }
        }

        public override IEnumerable<IPackage> GetPackages()
        {
            return (from file in FileSystem.GetFiles("", "*" + Constants.PackageExtension)
                    let packageName = Path.GetFileNameWithoutExtension(file)
                    where FileSystem.DirectoryExists(packageName)
                    select new UnzippedPackage(FileSystem, packageName)).AsQueryable();
        }

        public override IPackage GetPackage(string packageId, INuGetVersion version)
        {
            string packageName = GetPackageFileName(packageId, version); 
            if (Exists(packageId, version))
            {
                return new UnzippedPackage(FileSystem, packageName);
            }
            return null;
        }

        public override IEnumerable<IPackage> GetPackages(string packageId)
        {
            return GetPackages().Where(p => p.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Exists(string packageId, INuGetVersion version)
        {
            string packageName = GetPackageFileName(packageId, version);
            string packageFile = packageName + Constants.PackageExtension;
            return FileSystem.FileExists(packageFile) && FileSystem.DirectoryExists(packageName);
        }

        private static string GetPackageFileName(string packageId, INuGetVersion version)
        {
            return packageId + "." + version.ToString();
        }
    }
}