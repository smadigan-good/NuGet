using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IPackageHashMetadata
    {

        string PackageHash
        {
            get;
            set;
        }

        string PackageHashAlgorithm
        {
            get;
            set;
        }

    }
}
