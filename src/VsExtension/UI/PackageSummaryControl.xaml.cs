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
    /// Interaction logic for PackageSummaryControl.xaml
    /// </summary>
    public partial class PackageSummaryControl : UserControl
    {
        public IPackage Package { get; private set; }

        private bool _installed;

        public PackageSummaryControl(IPackage package, bool installed)
        {
            InitializeComponent();

            Package = package;
            _installed = installed;
            _id.Text = Package.Id;
            if (_installed)
            {
                _id.Foreground = new SolidColorBrush(Colors.Green);
                _id.FontStyle = FontStyles.Italic;
            }
            _tags.Text = "Tags: " + Package.Tags;
            _summary.Text = Package.Summary;

            if (Package.IconUrl != null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = Package.IconUrl;
                bi.EndInit();
                _icon.Source = bi;
            }
        }
    }
}
