
namespace NuGet
{
    public interface IPackageName
    {
        string Id { get; }

        INuGetVersion Version { get; }
    }
}
