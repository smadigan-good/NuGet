using System.ComponentModel.Composition;

namespace NuGet.ShimV3
{
    /// <summary>
    /// Ensures that there is only one instance of the shim controller.
    /// </summary>
    [Export(typeof(IShimControllerProvider))]
    public class ShimControllerProvider : IShimControllerProvider
    {
        private IShimController _controller;

        public IShimController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller = new ShimController();
                }

                return _controller;
            }
        }
    }
}
