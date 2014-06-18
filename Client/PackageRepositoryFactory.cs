using System;

namespace NuGet
{

    // Deprecated wrapper for ClientRepositoryProvider
    public class PackageRepositoryFactory : IPackageRepositoryFactory
    {
        //private static readonly PackageRepositoryFactory _default = new PackageRepositoryFactory();
        //private static readonly Func<Uri, IHttpClient> _defaultHttpClientFactory = u => new RedirectedHttpClient(u);
        //private Func<Uri, IHttpClient> _httpClientFactory;

        public static IPackageRepositoryFactory Default
        {
            get
            {
                return ClientRepositoryProvider.Default;
            }
        }

        //public Func<Uri, IHttpClient> HttpClientFactory
        //{
        //    get { return _httpClientFactory ?? _defaultHttpClientFactory; }
        //    set { _httpClientFactory = value; }
        //}

        public virtual IPackageRepository CreateRepository(string packageSource)
        {
            return Default.CreateRepository(packageSource);

            //throw new NotImplementedException();
            //if (packageSource == null)
            //{
            //    throw new ArgumentNullException("packageSource");
            //}

            //Uri uri = new Uri(packageSource);
            //if (uri.IsFile)
            //{
            //    return new LocalPackageRepository(uri.LocalPath);
            //}

            //var client = HttpClientFactory(uri);

            //// Make sure we get resolve any fwlinks before creating the repository
            //return new DataServicePackageRepository(client);
        }
    }
}
