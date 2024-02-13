using Microsoft.VisualStudio.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using Community.VisualStudio.Toolkit;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using Debricked.Helpers;
using Debricked.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debricked.Models.Constants;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Shell.Interop;

namespace Debricked.toolwindows.main.Controls
{
    /// Interaction logic for tw_mainControl.
    public partial class tw_mainControl : UserControl
    {
        private string packageName = "Debricked";
        private ScanResult currentScanResult = null;
        private General settings = null;
        private CollectionViewSource vulnerabilities = new CollectionViewSource();
        private CollectionViewSource dependencies = new CollectionViewSource();
        private ObservableCollection<Vulnerability> vulnerabilitiesCollection = new ObservableCollection<Vulnerability>();
        private ObservableCollection<Dependency> dependenciesCollection = new ObservableCollection<Dependency>();

        private bool scanRunning = false;
        /// Initializes a new instance of the <see cref="tw_mainControl"/> class.
        public tw_mainControl()
        {
            VS.Events.SolutionEvents.OnAfterOpenSolution += SolutionEvents_OnAfterOpenSolution;
            VS.Events.SolutionEvents.OnAfterOpenProject += SolutionEvents_OnAfterOpenProject;

            this.InitializeComponent();
            this.settings = ThreadHelper.JoinableTaskFactory.Run(() => General.GetLiveInstanceAsync());
            this.vulnerabilities.Source = this.vulnerabilitiesCollection;
            this.dgVulns.DataContext = vulnerabilities.View;
            this.dependencies.Source = this.dependenciesCollection;
            this.dgDeps.DataContext = dependencies.View;
            loadScanResults();
        }

        public void SolutionEvents_OnAfterOpenProject(Project obj)
        {
            if (!settings.ScanSolution)
            {
                loadScanResults();
            }
        }

        public void SolutionEvents_OnAfterOpenSolution(Solution obj)
        {
            if (settings.ScanSolution)
            {
                loadScanResults();
            }
        }

        private void loadScanResults()
        {
            if (settings.ScanSolution)
            {
                var sr = ScanResultStorageHelper.Load(VS.Solutions.GetCurrentSolution(), settings.DataDir);
                if (sr != null) { updateItemSources(sr); } else { clearItemSources(); }
            }
            else
            {
                var sr = ScanResultStorageHelper.Load(ThreadHelper.JoinableTaskFactory.Run(() => VS.Solutions.GetActiveProjectAsync()), settings.DataDir);
                if (sr != null) { updateItemSources(sr); } else { clearItemSources(); }
            }
        }

        private void updateItemSources(ScanResult scanResult)
        {
            this.currentScanResult = scanResult;
            if (scanResult != null)
            {
                this.vulnerabilitiesCollection.Clear();
                foreach (var vuln in scanResult.Vulnerabilities)
                {
                    this.vulnerabilitiesCollection.Add(vuln.Value);
                }
                this.dependenciesCollection.Clear();
                foreach (var dependency in scanResult.Dependencies)
                {
                    this.dependenciesCollection.Add(dependency.Value);
                }
                //force datagrid sorting
                this.vulnerabilities.View.SortDescriptions.Clear();
                this.vulnerabilities.View.SortDescriptions.Add(new SortDescription("Cvss", ListSortDirection.Descending));
                this.vulnerabilities.View.Refresh();

                this.dependencies.View.SortDescriptions.Clear();
                this.dependencies.View.SortDescriptions.Add(new SortDescription("PolicyStatus", ListSortDirection.Descending));
                this.dependencies.View.Refresh();

                this.tbScope.Text = String.Format("Scope: {0}", scanResult.Scope);
                this.tbLastRefresh.Text = String.Format("Last Refresh: {0}", scanResult.LastRefresh);
                this.btnRefresh.Content = "Rescan";
                this.btnRefresh2.Content = "Rescan";
                writeToErrorList(scanResult.Dependencies);
            } else
            {
                this.btnRefresh.Content = "Scan";
                this.btnRefresh2.Content = "Scan";
            }
        }

        private void clearItemSources()
        {
            this.vulnerabilitiesCollection.Clear();
            this.dependenciesCollection.Clear();
            this.vulnerabilities.View.Refresh();
            this.dependencies.View.Refresh();
            this.tbScope.Text = string.Empty;
            this.tbLastRefresh.Text = string.Empty;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!scanRunning)
            {
                ThreadHelper.JoinableTaskFactory.Run(RunScanAsync);
            }
        }

        public async Task RunScanAsync()
        {
            scanRunning = true;
            await DebrickedCliHelper.updateDebrickedCliAsync(settings);
            if (settings.ScanSolution)
            {
                //big repo: 76682, 2212740
                //smaller repo: 80953, 2365522
                //var sr =await ScanHelper.finishScanAsync(null, 80953, 2365522, settings, this.currentScanResult);
                //var sr = await ScanHelper.finishScanAsync(null, 76682, 2212740, settings, this.currentScanResult);
                ScanHelper scanHelper = new ScanHelper(await VS.Solutions.GetCurrentSolutionAsync(), null, settings, currentScanResult);
                ScanResult sr = await scanHelper.RunScanAsync();
                updateItemSources(sr);
                ScanResultStorageHelper.Store(sr, settings.DataDir);

            }
            else
            {
                ScanHelper scanHelper = new ScanHelper(null, await VS.Solutions.GetActiveProjectAsync(), settings, currentScanResult);
                var sr = await scanHelper.RunScanAsync();
                updateItemSources(sr);
                ScanResultStorageHelper.Store(sr, settings.DataDir);
            }
            await VS.StatusBar.ShowMessageAsync("");
            await VS.StatusBar.ClearAsync();
            scanRunning = false;
        }

        private void btnFilterVulnerabilities_Click(object sender, RoutedEventArgs e)
        {
            filterVulnerabilities();
        }

        private void btnFilterDependencies_Click(object sender, RoutedEventArgs e)
        {
            filterDependencies();
        }

        private void btnFilterDependenciesClear_Click(object sender, RoutedEventArgs e)
        {
            this.tbFilterDependencies.Text = String.Empty;
            dependencies.View.Filter = null;
            dependencies.View.Refresh();
        }

        private void btnFilterVulnerabilitiesClear_Click(object sender, RoutedEventArgs e)
        {
            this.tbFilterVulnerabilities.Text= String.Empty;
            vulnerabilities.View.Filter = null;
            vulnerabilities.View.Refresh();
        }

        private void tbFilterDependencies_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                filterDependencies();
            }
        }
        private void tbFilterVulnerabilities_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                filterVulnerabilities();
            }
        }

        private void filterDependencies()
        {
            string filterText = this.tbFilterDependencies.Text;
            if (!String.IsNullOrEmpty(filterText))
            {
                dependencies.View.Filter = new Predicate<object>((item) => ((Dependency)item).Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase)>=0);
            }
            else
            {
                dependencies.View.Filter = null;
            }
            dependencies.View.Refresh();
        }

        private void filterVulnerabilities()
        {
            string filterText = this.tbFilterVulnerabilities.Text;
            if (!String.IsNullOrEmpty(filterText))
            {
                vulnerabilities.View.Filter = new Predicate<object>((item) => ((Vulnerability)item).CveId.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            {
                vulnerabilities.View.Filter = null;
            }
            vulnerabilities.View.Refresh();
        }

        private void writeToErrorList(ConcurrentDictionary<int, Dependency> dependencies)
        {
            var errorListProvider = ErrorListHelper.ErrorListHelperInstance;
            errorListProvider.Tasks.Clear();
            foreach (var dependency in dependencies)
            {
                switch (dependency.Value.PolicyStatus)
                {
                    case PolicyStatusEnum.Warn:
                        errorListProvider.Tasks.Add(new ErrorTask
                        {
                            ErrorCategory = TaskErrorCategory.Warning,
                            Category = TaskCategory.Misc,
                            Text = dependency.Value.GetErrorListDescription(),
                            Document = String.Join(", ", dependency.Value.IntroducedThroughFiles),
                        });
                        break;
                    case PolicyStatusEnum.Fail:
                        errorListProvider.Tasks.Add(new ErrorTask
                        {
                            ErrorCategory = TaskErrorCategory.Error,
                            Category = TaskCategory.Misc,
                            Text = dependency.Value.GetErrorListDescription(),
                            Document = String.Join(", ", dependency.Value.IntroducedThroughFiles),
                        });
                        break;
                    case PolicyStatusEnum.Pass:
                    default:
                        continue;
                }
            }
            Console.WriteLine(errorListProvider.Tasks.Count);
            //errorListProvider.ForceShowErrors();
        }
    }
}