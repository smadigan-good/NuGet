using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NuGet.Tools
{
    /// <summary>
    /// Interaction logic for PackageDetail.xaml
    /// </summary>
    public partial class PackageDetail : UserControl
    {
        private IPackage _package;

        public IPackage Package
        {
            get { return _package; }
            set
            {
                _package = value;
                Refresh();
            }
        }

        public PackageDetail()
        {
            InitializeComponent();
        }

        // Refresh UI
        private void Refresh()
        {
            _id.Text = _package.ToString();
            _description.Text = _package.Description;
        }
    }
}
