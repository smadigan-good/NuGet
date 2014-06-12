using NuGet.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public class SimpleRepository : FileSystemRepository
    {

        public SimpleRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : base(pathResolver, fileSystem)
        {

        }

        public override IQueryable<IPackage> GetPackages()
        {
            return GetPackageFiles().Select(path => OpenPackage(path)).AsQueryable();
        }

        /// <summary>
        /// Open a nupkg or nuspec file
        /// </summary>
        protected virtual IPackage OpenPackage(string path)
        {
            IPackage package = null;

            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension == Constants.PackageExtension)
            {
                OptimizedZipPackage zip = null;

                try
                {
                    zip = new OptimizedZipPackage(FileSystem, path);
                }
                catch (FileFormatException ex)
                {
                    throw new InvalidDataException(String.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingPackage, path), ex);
                }

                // Set the last modified date on the package
                zip.Published = FileSystem.GetLastModified(path);

                package = zip;
            }
            else if (extension == Constants.ManifestExtension)
            {
                package = new UnzippedPackage(FileSystem, Path.GetFileNameWithoutExtension(path));
            }

            return package;
        }

        protected virtual string GetPackageFilePath(IPackage package)
        {
            return Path.Combine(PathResolver.GetPackageDirectory(package),
                                PathResolver.GetPackageFileName(package));
        }

        protected virtual string GetPackageFilePath(IPackage package, SemanticVersion version)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetPackageFilePath(string id, SemanticVersion version)
        {
            throw new NotImplementedException();
        }

        protected virtual IEnumerable<string> GetPackageFiles()
        {
            throw new NotImplementedException();
        }

    }
}
