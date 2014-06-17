using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    /// <summary>
    /// A hybrid model of SemVer that supports both semantic versioning as described at http://semver.org, and older 4-digit versioning schemes.
    /// </summary>
    public interface INuGetVersion : IComparable, IComparable<INuGetVersion>, IEquatable<INuGetVersion>
    {
        /// <summary>
        /// Major version X (X.y.z)
        /// </summary>
        int Major { get; }

        /// <summary>
        /// Minor version Y (x.Y.z)
        /// </summary>
        int Minor { get; }

        /// <summary>
        /// Patch version Z (x.y.Z)
        /// </summary>
        int Patch { get; }

        /// <summary>
        /// Fourth version digit for legacy versions
        /// </summary>
        int Revision { get; }

        /// <summary>
        /// A collection of pre-release labels attached to the version.
        /// </summary>
        IEnumerable<string> ReleaseLabels { get; }

        /// <summary>
        /// The full pre-release label for the version.
        /// </summary>
        string Release { get; }

        /// <summary>
        /// True if pre-release labels exist for the version.
        /// </summary>
        bool IsPrerelease { get; }

        /// <summary>
        /// True if metadata exists for the version.
        /// </summary>
        bool HasMetadata { get; }

        /// <summary>
        /// Build metadata attached to the version.
        /// </summary>
        string Metadata { get; }

        /// <summary>
        /// .NET version type for the INuGetVersion object
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// True if the INuGetVersion objects are equal based on the given comparison mode.
        /// </summary>
        bool Equals(INuGetVersion other, VersionComparison versionComparison);

        /// <summary>
        /// Compares INuGetVersion objects using the given comparison mode.
        /// </summary>
        int CompareTo(INuGetVersion other, VersionComparison versionComparison);

        /// <summary>
        /// Gives a normalized representation of the version.
        /// </summary>
        string ToNormalizedString();
    }
}
