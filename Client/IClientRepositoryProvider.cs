using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IClientRepositoryProvider : IPackageRepositoryFactory
    {
        bool TryGetRepository(string source, out IPackageRepository repository);

        bool TryCreateLocalStore(out ILocalStore localStore);
    }
}
