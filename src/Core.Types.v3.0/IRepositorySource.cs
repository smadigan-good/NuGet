using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IRepositorySource : IEquatable<IRepositorySource>, IComparable<IRepositorySource>
    {
        /// <summary>
        /// Highest comes first
        /// </summary>
        double Priority { get; }

        /// <summary>
        /// Display name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// URI, path, or anything that can be understood by an IPackageRepositoryProvider
        /// </summary>
        string Source { get; }
    }
}
