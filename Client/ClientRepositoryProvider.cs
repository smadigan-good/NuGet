using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    [Export(typeof(IClientRepositoryProvider))]
    public class ClientRepositoryProvider : IClientRepositoryProvider
    {
        [ImportMany]
        private IEnumerable<IPackageRepositoryProvider> _repositoryProviders = null;

        public ClientRepositoryProvider()
        {

        }

        public static IClientRepositoryProvider Default
        {
            get
            {
                return new ClientRepositoryProvider();
            }
        }

        public bool TryGetRepository(string source, out IPackageRepository repository)
        {
            foreach (var provider in _repositoryProviders)
            {
                if (provider.TryCreateRepository(source, out repository))
                {
                    return true;
                }
            }

            repository = null;
            return false;
        }

        public bool TryCreateLocalStore(out ILocalStore localStore)
        {
            throw new NotImplementedException();
        }

        public IPackageRepository CreateRepository(string packageSource)
        {
            throw new NotImplementedException();
        }

    }
}
