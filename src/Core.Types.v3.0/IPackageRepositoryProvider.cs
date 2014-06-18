using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IPackageRepositoryProvider
    {
        bool IsSourceSupported(string source);

        bool TryCreateRepository(string source, out IPackageRepository repository);
    }
}
