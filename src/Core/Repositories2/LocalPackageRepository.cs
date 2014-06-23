using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class LocalPackageRepository : FileSystemRepository
    {
        // Adds caching to simple repository

        public LocalPackageRepository(string path)
            : this(new DefaultPackagePathResolver(path), new PhysicalFileSystem(path))
        {

        }

        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(pathResolver, fileSystem, true)
        {

        }


        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem, bool enableCaching)
            : base(pathResolver, fileSystem)
        {

        }

        public bool TryGetLatestPackageVersion(string packageId, out SemanticVersion version)
        {
            IPackage package = null;
            if (TryGetLatestPackage(packageId, true, true, out package))
            {
                version = package.Version.ToSemanticVersion();
                return true;
            }

            version = null;
            return false;
        }

        protected override IEnumerable<string> GetPackageFiles()
        {
            // Check for package files one level deep. We use this at package install time
            // to determine the set of installed packages. Installed packages are copied to 
            // {id}.{version}\{packagefile}.{extension}.
            var dirs = (new string[] { string.Empty }).Concat(FileSystem.GetDirectories(string.Empty));

            string filter = "*.nu*";

            foreach(string dir in dirs)
            {
                foreach (string path in FileSystem.GetFiles(dir, filter))
                {
                    if (path.EndsWith(Constants.PackageExtension) || path.EndsWith(Constants.ManifestExtension))
                    {
                        yield return path;
                    }
                }
            }
        }
    }
}
