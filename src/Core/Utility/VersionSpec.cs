using System.Globalization;
using System.Text;

namespace NuGet
{
    public class VersionSpec : IVersionSpec
    {
        public VersionSpec()
        {
        }

        public VersionSpec(SemanticVersion version)
        {

        }
        public VersionSpec(INuGetVersion version)
        {

        }

        public VersionSpec(INuGetVersion minVersion, bool includeMin)
        {

        }

        public VersionSpec(INuGetVersion minVersion, bool includeMin, INuGetVersion maxVersion, bool includeMax)
        {

        }

        public INuGetVersion MinVersion
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public bool IsMinInclusive
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public INuGetVersion MaxVersion
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public bool IsMaxInclusive
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public bool Satisfies(INuGetVersion version)
        {
            throw new System.NotImplementedException();
        }

        public bool Satisfies(INuGetVersion version, VersionComparison versionComparison)
        {
            throw new System.NotImplementedException();
        }

        public bool Satisfies(INuGetVersion version, IVersionComparer comparer)
        {
            throw new System.NotImplementedException();
        }

        public string PrettyPrint()
        {
            throw new System.NotImplementedException();
        }

        public string ToNormalizedString()
        {
            throw new System.NotImplementedException();
        }
    }
}