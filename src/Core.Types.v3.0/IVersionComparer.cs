using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    /// <summary>
    /// IVersionComparer represents a version comparer capable of sorting and determining the equality of INuGetVersion objects.
    /// </summary>
    public interface IVersionComparer : IEqualityComparer<INuGetVersion>, IComparer<INuGetVersion>
    {

    }
}
