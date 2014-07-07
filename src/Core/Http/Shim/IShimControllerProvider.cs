
namespace NuGet
{
    /// <summary>
    /// Provides the IShimController instance for the v3 client.
    /// </summary>
    public interface IShimControllerProvider
    {
        IShimController Controller { get; }
    }
}
