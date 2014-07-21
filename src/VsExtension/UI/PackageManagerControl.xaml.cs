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
using NuGet.Resolver;
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
        IVsPackageManagerFactory _packageManagerFactory;
        IPackageRepository _localRepo;

        public PackageManagerControl()
        {
            _packageSourceProvider = ServiceLocator.GetInstance<IVsPackageSourceProvider>();
            _packageRepoFactory = ServiceLocator.GetInstance<IPackageRepositoryFactory>();
            _packageManagerFactory = ServiceLocator.GetInstance<IVsPackageManagerFactory>();

            InitializeComponent();
            Update();
        }

        public void SetProject(Project project)
        {
            _project = project;

            Update();
        }

        public Project Project
        {
            get { return _project; }
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

            if (_project != null)
            {
                var packageManager = _packageManagerFactory.CreatePackageManagerToManageInstalledPackages();
                var projectManager = packageManager.GetProjectManager(_project);

                _localRepo = projectManager.LocalRepository;

                var installedPackages = _localRepo.GetPackages().ToList();
                _packageList.Items.Clear();
                foreach (var package in installedPackages)
                {
                    var packageSummaryControl = new PackageSummaryControl(package, installed: true);
                    packageSummaryControl.Margin = new Thickness(0, 4, 0, 4);
                    _packageList.Items.Add(packageSummaryControl);
                }
            }
        }

        private async Task<List<IPackage>> GetPackagesAsync(Func<List<IPackage>> func)
        {
            List<IPackage> retValue = null;
            await Task.Factory.StartNew(
                () =>
                {
                    retValue = func();
                });
            return retValue;
        }

        private async void SearchPackageInActivePackageSource()
        {
            string targetFramework = _project.GetTargetFramework();                    
            _packageList.Items.Clear();
            _searchButton.IsEnabled = false;
            _busyControl.Visibility = System.Windows.Visibility.Visible;
            var searchText = _searchText.Text;

            var installedPackages = _localRepo.GetPackages().ToList();
            var onlinePackages = await GetPackagesAsync(
                () => {
                    var repo = _packageRepoFactory.CreateRepository(_packageSourceProvider.ActivePackageSource.Name);
                    var supportedFrameWorks = targetFramework != null ? new[] { targetFramework } : new string[0];
                    var query = repo.Search(
                        searchTerm: searchText,
                        targetFrameworks: supportedFrameWorks,
                        allowPrereleaseVersions: false);
                    query = query.Take(30);
                    return query.ToList();
                });
            
            _busyControl.Visibility = System.Windows.Visibility.Hidden;
            foreach (var package in installedPackages)
            {
                var packageSummaryControl = new PackageSummaryControl(package, installed: true);
                packageSummaryControl.Margin = new Thickness(0, 4, 0, 4);
                _packageList.Items.Add(packageSummaryControl);
            }

            foreach (var package in onlinePackages)
            {
                var packageSummaryControl = new PackageSummaryControl(package, installed: false);
                packageSummaryControl.Margin = new Thickness(0, 4, 0, 4);
                _packageList.Items.Add(packageSummaryControl);
            }
            _searchButton.IsEnabled = true;
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {           
            SearchPackageInActivePackageSource();
        }

        private void PackageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPackage = _packageList.SelectedItem as PackageSummaryControl;
            if (selectedPackage != null)
            {
                _packageDetail.Package = selectedPackage.Package;
            }
            else
            {
                _packageDetail.Package = null;
            }
        }

        private void InstallButtonClicked(object sender, EventArgs e)
        {

        }

        private void InstallPreviewButtonClicked(object sender, EventArgs e)
        {
            // !!!
            var package = _packageDetail.Package;
            var repo = _packageRepoFactory.CreateRepository(_packageSourceProvider.ActivePackageSource.Name);
            IVsPackageManager activePackageManager = _packageManagerFactory.CreatePackageManager(
                repo, useFallbackForDependencies: true);

            // Resolve operations
            var resolver = new ActionResolver()
            {
                DependencyVersion = activePackageManager.DependencyVersion,
                IgnoreDependencies = false,
                AllowPrereleaseVersions = false
            };
            var projectManager = activePackageManager.GetProjectManager(_project);
            resolver.AddOperation(PackageAction.Install, package, projectManager);
            var actions = resolver.ResolveActions();

            // Show result
            // values:
            // 1: unchanged
            // 0: deleted
            // 2: added
            var packageStatus = new Dictionary<IPackage,int>(PackageEqualityComparer.IdAndVersion);
            foreach(var p in projectManager.LocalRepository.GetPackages())
            {
                packageStatus[p] = 1;
            }

            foreach (var action in actions)
            {
                var projectAction = action as PackageProjectAction;
                if (projectAction == null)
                {
                    continue;
                }

                if (projectAction.ActionType == PackageActionType.Install)
                {
                    packageStatus[projectAction.Package] = 2;
                }
                else if (projectAction.ActionType == PackageActionType.Uninstall)
                {
                    packageStatus[projectAction.Package] = 0;
                }
            }

            var w = new InstallPreviewWindow(
                unchanged: packageStatus.Where(v => v.Value == 1).Select(v => v.Key),
                deleted: packageStatus.Where(v => v.Value == 0).Select(v => v.Key),
                added: packageStatus.Where(v => v.Value == 2).Select(v => v.Key));
            w.ShowDialog();
        }
    }
}
