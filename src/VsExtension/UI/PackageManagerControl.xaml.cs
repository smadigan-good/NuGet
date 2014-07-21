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

        private async Task<List<IPackage>> GetPackagesAsync(IQueryable<IPackage> q)
        {
            List<IPackage> retValue = null;
            await Task.Factory.StartNew(
                () =>
                {
                    retValue = q.ToList();
                });
            return retValue;
        }

        private async void SearchPackageInActivePackageSource(/* searchText */)
        {
            var repo = _packageRepoFactory.CreateRepository(_packageSourceProvider.ActivePackageSource.Name);

            _project.GetTargetFramework();
            string targetFramework = _project.GetTargetFramework();
            var supportedFrameWorks = targetFramework != null ? new[] { targetFramework } : new string[0];            
            var query = repo.Search(
                searchTerm: _searchText.Text,
                targetFrameworks: supportedFrameWorks,
                allowPrereleaseVersions: false);
            query = query.Take(30);

            var packages = await GetPackagesAsync(query);
            _packageList.Items.Clear();
            foreach (var package in packages)
            {
                // !!!
                _packageList.Items.Add(new PackageSummaryControl(package));
            }            
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            SearchPackageInActivePackageSource();
        }

        private void PackageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPackage = _packageList.SelectedItem as PackageSummaryControl;
            _packageDetail.Package = selectedPackage.Package;
        }
    }
}
