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

        public event EventHandler InstallPreviewButtonClicked;
        public event EventHandler InstallButtonClicked;

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

            _installButton.Click += (obj, e) => { InstallButtonClicked(this, e); };
            _installPreviewButton.Click += (obj, e) => { InstallPreviewButtonClicked(this, e); };
        }

        // Refresh UI
        private void Refresh()
        {
            _dependencies.Items.Clear();

            if (_package == null)
            {
                _id.Text = "";
                _description.Text = "";
                _installButton.IsEnabled = false;
                _installPreviewButton.IsEnabled = false;
            }
            else
            {
                _id.Text = _package.ToString();
                _description.Text = _package.Description;
                _installButton.IsEnabled = true;
                _installPreviewButton.IsEnabled = true;

                foreach (var dependencySet in _package.DependencySets)
                {
                    if (dependencySet.TargetFramework != null)
                    {
                        _dependencies.Items.Add(new TextBlock()
                        {
                            Text = dependencySet.TargetFramework.ToString(),
                            FontWeight = FontWeights.DemiBold,
                            Margin = new Thickness(10, 0, 0, 0)
                        });
                    }

                    foreach (var d in dependencySet.Dependencies)
                    {
                        _dependencies.Items.Add(new TextBlock()
                            {
                                Text = d.ToString(),
                                TextWrapping = System.Windows.TextWrapping.Wrap,
                                Margin = new Thickness(20, 0, 0, 0)
                            });
                    }
                }
            }
        }
    }
}
