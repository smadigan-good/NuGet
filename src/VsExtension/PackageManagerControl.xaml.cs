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
using EnvDTE;
using NuGet.VisualStudio;

namespace NuGet.Tools
{
    /// <summary>
    /// Interaction logic for PackageManagerControl.xaml
    /// </summary>
    public partial class PackageManagerControl : UserControl
    {
        private Project _project;

        IVsPackageSourceProvider _packageSourceProvider;
        IPackageRepositoryFactory _packageRepoFactory;

        public PackageManagerControl()
        {
            _packageSourceProvider = ServiceLocator.GetInstance<IVsPackageSourceProvider>();
            _packageRepoFactory = ServiceLocator.GetInstance<IPackageRepositoryFactory>();

            InitializeComponent();
            Update();
        }

        public void SetProject(Project project)
        {
            _project = project;

            Update();
        }

        private void Update()
        {
            // init source repo list
            _sourceRepoList.Items.Clear();
            var sources = _packageSourceProvider.GetEnabledPackageSourcesWithAggregate().Select(ps => ps.Name);
            foreach (var source in sources)
            {
                _sourceRepoList.Items.Add(source);
            }
            _sourceRepoList.SelectedItem = _packageSourceProvider.ActivePackageSource.Name;


            // !!!            
        }

        private void SearchPackageInActivePackageSource(/* searchText */)
        {
            var repo = _packageRepoFactory.CreateRepository(_packageSourceProvider.ActivePackageSource.Name);
            var packages = repo.GetPackages().OrderByDescending(p => p.DownloadCount).Take(30);
            _packageList.Items.Clear();
            foreach (var package in packages)
            {
                // !!!
                _packageList.Items.Add(package.ToString());
            }            
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            SearchPackageInActivePackageSource();
        }
    }
}
