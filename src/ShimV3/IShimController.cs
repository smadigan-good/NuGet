using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IShimController
    {
        void Enable(IPackageSourceProvider sourceProvider);

        void UpdateSources();

        void Disable();
    }
}
