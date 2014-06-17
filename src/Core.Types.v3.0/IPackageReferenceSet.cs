using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IPackageReferenceSet : IFrameworkTargetable
    {
        IEnumerable<string> References { get; }

        FrameworkName TargetFramework { get; }
    }
}
