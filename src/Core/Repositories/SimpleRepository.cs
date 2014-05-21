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
    /*
    public class SimpleRepository : PackageRepositoryBase, IPackageLookup
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackagePathResolver _pathResolver;
        private readonly bool _enableCaching;
        private readonly string _filter = String.Format(CultureInfo.InvariantCulture, "*{0}", Constants.PackageExtension);

        public SimpleRepository(string physicalPath)
            : this(physicalPath, enableCaching: true)
        {

        }

        public SimpleRepository(string physicalPath, bool enableCaching)
            : this(new DefaultPackagePathResolver(physicalPath),
                   new PhysicalFileSystem(physicalPath),
                   enableCaching)
        {

        }

        public SimpleRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(pathResolver, fileSystem, enableCaching: true)
        {

        }

        public SimpleRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem, bool enableCaching)
        {
            if (pathResolver == null)
            {
                throw new ArgumentNullException("pathResolver");
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            _fileSystem = fileSystem;
            _pathResolver = pathResolver;
            _enableCaching = enableCaching;
        }

        public override IPackage FindPackage(string packageId, SemanticVersion version)
        {
            if (String.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            return GetPackages().Where(p => p.Id == packageId && p.Version == version).FirstOrDefault();
        }

        public override IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            if (String.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }

            return GetPackages().Where(p => p.Id == packageId);
        }

        public override bool Exists(string packageId, SemanticVersion version)
        {
            return FindPackage(packageId, version) != null;
        }


        public override bool SupportsPrereleasePackages
        {
            get { return true; }
        }

        public override string Source
        {
            get { return _fileSystem.Root; }
        }

        public override IQueryable<IPackage> GetPackages()
        {
            return GetPackages(OpenPackage).AsQueryable();
        }

        private IEnumerable<IPackage> GetPackages(Func<string, IPackage> openPackage)
        {
            return from path in GetPackageFiles()
                   select GetPackage(openPackage, path);
        }

        protected virtual IPackage GetPackage(Func<string, IPackage> openPackage, string path)
        {
            // TODO: Caching

            return openPackage(path);
        }

        protected virtual IPackage OpenPackage(string path)
        {
            OptimizedZipPackage zip = null;

            if (_fileSystem.FileExists(path))
            {
                try
                {
                    zip = new OptimizedZipPackage(_fileSystem, path);
                }
                catch (FileFormatException ex)
                {
                    throw new InvalidDataException(String.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingPackage, path), ex);
                }
                // Set the last modified date on the package
                zip.Published = _fileSystem.GetLastModified(path);
            }

            return zip;
        }

        /// <summary>
        /// *.nupkg files in the root folder
        /// </summary>
        protected virtual IEnumerable<string> GetPackageFiles()
        {
            // Check top level directory
            foreach (var path in _fileSystem.GetFiles(String.Empty, _filter))
            {
                yield return path;
            }
        }
    } */
}
