using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
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
