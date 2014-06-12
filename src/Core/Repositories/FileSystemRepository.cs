using System;
using System.Collections.Generic;
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

        public override void AddPackage(IPackage package)
        {
            // TODO: add from local repository
            throw new NotImplementedException();
        }

        public override void RemovePackage(IPackage package)
        {
            throw new NotImplementedException();
        }

    }
}
