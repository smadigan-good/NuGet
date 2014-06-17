using System;
using System.Collections.Generic;

namespace NuGet
{
    public interface IPackageMetadata : IPackageFormalName
    {
        string Title { get; }
        IEnumerable<string> Authors { get; }
        IEnumerable<string> Owners { get; }
        Uri IconUrl { get; }
        Uri LicenseUrl { get; }
        Uri ProjectUrl { get; }
        bool RequireLicenseAcceptance { get; }
        bool DevelopmentDependency { get; }
        string Description { get; }
        string Summary { get; }
        string ReleaseNotes { get; }
        string Language { get; }
        string Tags { get; }
        string Copyright { get; }

        /// <summary>
        /// Specifies assemblies from GAC that the package depends on.
        /// </summary>
        IEnumerable<IFrameworkAssemblyReference> FrameworkAssemblies { get; }
        
        /// <summary>
        /// Returns sets of References specified in the manifest.
        /// </summary>
        IEnumerable<IPackageReferenceSet> PackageAssemblyReferences { get; }

        /// <summary>
        /// Specifies sets other packages that the package depends on.
        /// </summary>
        IEnumerable<IPackageDependencySet> DependencySets { get; }

        Version MinClientVersion { get; }
    }
}