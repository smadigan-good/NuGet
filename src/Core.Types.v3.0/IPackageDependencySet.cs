using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IPackageDependencySet : IFrameworkTargetable
    {
        IEnumerable<IPackageDependency> Dependencies { get; }

        FrameworkName TargetFramework { get; }
    }
}
