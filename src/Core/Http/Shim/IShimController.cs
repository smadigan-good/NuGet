
using System;

namespace NuGet
{
    public interface IShimController
    {
        void Enable(IPackageSourceProvider sourceProvider);

        void UpdateSources();

        void Disable();
    }
}
