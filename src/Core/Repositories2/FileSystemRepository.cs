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
    public abstract class FileSystemRepository : PackageRepositoryBase
    {
        private IPackagePathResolver _resolver;
        private IFileSystem _fileSystem;

        public FileSystemRepository(IPackagePathResolver resolver, IFileSystem fileSystem)
            : base()
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            _fileSystem = fileSystem;
            _resolver = resolver;
        }

        public IPackagePathResolver PathResolver
        {
            get
            {
                return _resolver;
            }
        }

        protected IFileSystem FileSystem
        {
            get
            {
                return _fileSystem;
            }
        }

        public override string Source
        {
            get
            {
                return FileSystem.Root;
            }
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
                package = OpenNupkg(path);
            }
            else if (extension == Constants.ManifestExtension)
            {
                package = OpenNuspec(path);
            }

            return package;
        }

        protected virtual IPackage OpenNuspec(string path)
        {
            return new UnzippedPackage(FileSystem, Path.GetFileNameWithoutExtension(path));
        }

        protected virtual IPackage OpenNupkg(string path)
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

            return zip;
        }

        public override IEnumerable<IPackage> GetPackages()
        {
            return GetPackageFiles().Select(path => OpenPackage(path));
        }

        protected virtual string GetPackageFilePath(IPackage package)
        {
            return Path.Combine(PathResolver.GetPackageDirectory(package),
                                PathResolver.GetPackageFileName(package));
        }

        // protected abstract string GetPackageFilePath(IPackage package, SemanticVersion version);

        protected virtual string GetPackageFilePath(string id, SemanticVersion version)
        {
            return Path.Combine(PathResolver.GetPackageDirectory(id, version),
                                PathResolver.GetPackageFileName(id, version));
        }

        protected abstract IEnumerable<string> GetPackageFiles();
    }
}
