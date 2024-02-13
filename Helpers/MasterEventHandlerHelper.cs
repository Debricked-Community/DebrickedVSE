using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VSLangProj;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using Debricked.Models.Constants;
using Microsoft.VisualStudio.Threading;
using EnvDTE;
using Debricked.toolwindows.main.Controls;
using Microsoft.VisualStudio;
using System.Collections.Concurrent;
using Debricked.Models;

namespace Debricked.Helpers
{
    internal class MasterEventHandlerHelper
    {
        private static MasterEventHandlerHelper instance;
        private General settings;
        private DebrickedPackage package;
        private HierarchyEventsHelper hierarchyEventsHelper;
        private List<uint> hierarchyEventsCookies = new List<uint>();
        private ReferencesEvents csRefEvents, vbRefEvents;
        private bool subbed = false;
        private bool debugSessionStarted = false;
        private DateTime solutionLoadTimestamp;
        private CommandEvents commandEvents;
        private EnvDTE80.Commands2 commands;
        private EventBurstHelper refAddedHelper;
        private MasterEventHandlerHelper(General _settings, DebrickedPackage _package)
        {
            settings = _settings;
            package = _package;
            refAddedHelper = new EventBurstHelper(() => { runScan(); }, 500);
            hierarchyEventsHelper = new HierarchyEventsHelper(this);
            VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += SolutionEvents_OnAfterBackgroundSolutionLoadComplete;

            //check if a solution is already loaded, if so update view / error list
            if (ThreadHelper.JoinableTaskFactory.Run(VS.Solutions.IsOpenAsync))
            {
                OnSolutionAlreadyLoaded();
                //SolutionEvents_OnAfterBackgroundSolutionLoadComplete();
            }
        }

        private void OnSolutionAlreadyLoaded()
        {
            if (settings.ScanSolution)
            {
                var solution = VS.Solutions.GetCurrentSolution();
                var toolwindow = package.FindToolWindow(typeof(tw_main), 0, false);
                if (toolwindow != null && toolwindow.Content != null)
                {
                    var toolWindowControl = toolwindow.Content as tw_mainControl;
                    toolWindowControl.Dispatcher.Invoke(() => toolWindowControl.SolutionEvents_OnAfterOpenSolution(solution));
                }
                else
                {
                    var sr = ScanResultStorageHelper.Load(solution, settings.DataDir);
                    if (sr != null)
                    {
                        writeToErrorList(sr.Dependencies);
                    }
                }
            }
            else
            {
                //TODO verify this works
                Community.VisualStudio.Toolkit.Project project = ThreadHelper.JoinableTaskFactory.Run<Community.VisualStudio.Toolkit.Project>(VS.Solutions.GetActiveProjectAsync);
                var toolwindow = package.FindToolWindow(typeof(tw_main), 0, false);
                if (toolwindow != null && toolwindow.Content != null)
                {
                    var toolWindowControl = toolwindow.Content as tw_mainControl;
                    toolWindowControl.Dispatcher.Invoke(() => toolWindowControl.SolutionEvents_OnAfterOpenProject(project));
                }
                else
                {
                    var sr = ScanResultStorageHelper.Load(project, settings.DataDir);
                    if (sr != null)
                    {
                        writeToErrorList(sr.Dependencies);
                    }
                }
            }
        }

        public static MasterEventHandlerHelper Instance { get { return instance; } }

        public static void Initialize(General _settings, DebrickedPackage _package)
        {
            if(instance == null) { instance = new MasterEventHandlerHelper(_settings, _package); }
        }

        public void SolutionEvents_OnAfterBackgroundSolutionLoadComplete()
        {
            var toolwindow = package.FindToolWindow(typeof(tw_main), 0, true);
            solutionLoadTimestamp = DateTime.Now;
            if (settings.Trigger_OnReferenceAdded)
            {
                try
                {
                    ThreadHelper.JoinableTaskFactory.Run(SubscribeToRefEventsAsync);
                }
                catch (Exception)
                {
                    //TODO
                    throw;
                }
            }

            if(settings.Trigger_OnBuild || settings.Trigger_OnBuildWithDebug)
            {
                VS.Events.BuildEvents.SolutionBuildDone += BuildEvents_SolutionBuildDone;
                if (!settings.Trigger_OnBuildWithDebug)
                {
                    //subscribe to command events to intercept Debug.Start and detect debug session
                    ThreadHelper.ThrowIfNotOnUIThread();
                    var dte = (EnvDTE80.DTE2)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE));
                    commandEvents = dte.Events.CommandEvents;
                    commands = dte.Commands as EnvDTE80.Commands2;
                    commandEvents.AfterExecute += CommandEvents_AfterExecute;
                }
            }
        }

        private void CommandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            string commandName = GetCommandName(Guid, ID);
            if(commandName == "Debug.Start")
            {
                debugSessionStarted = true;
            }
        }

        private void BuildEvents_SolutionBuildDone(bool obj)
        {
            if(settings.Trigger_OnBuildWithDebug || debugSessionStarted == false)
            {
                runScan();
            }
        }

        private void runScan()
        {
            debugSessionStarted = false;
            //TODO handle case where window is closed, should still work since the window is initialized in the background
            var toolwindow = package.FindToolWindow(typeof(tw_main), 0, false);
            if (toolwindow != null && toolwindow.Content != null)
            {
                var toolWindowControl = toolwindow.Content as tw_mainControl;
                ThreadHelper.JoinableTaskFactory.Run(() => toolWindowControl.Dispatcher.Invoke(toolWindowControl.RunScanAsync));
            }
        }

        public void SolutionEvents_OnAfterCloseSolution()
        {
            //TODO unsub handlers?? do we need the hierarchy item for that?
        }

        //TODO add error handling
        private async Task SubscribeToRefEventsAsync()
        {
            if (settings.ScanSolution)
            {
                foreach (var project in await VS.Solutions.GetAllProjectsAsync())
                {
                    await SubscribeToRefEventsAsync(project);
                }
            } else
            {
                await SubscribeToRefEventsAsync(await VS.Solutions.GetActiveProjectAsync());
            }
        }

        private async Task SubscribeToRefEventsAsync(Community.VisualStudio.Toolkit.Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (EnvDTE.DTE)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE));
            switch (CLHelper.GetProjectType(project))
            {
                case CLProjectTypeEnum.VbProj:
                    vbRefEvents = (ReferencesEvents)dte.Events.GetObject("VBReferencesEvents");
                    vbRefEvents.ReferenceAdded += new _dispReferencesEvents_ReferenceAddedEventHandler(HandleRefAdded);
                    vbRefEvents.ReferenceRemoved += new _dispReferencesEvents_ReferenceRemovedEventHandler(HandleRefRemoved);
                    break;
                case CLProjectTypeEnum.CSProj:
                    csRefEvents = (ReferencesEvents)dte.Events.GetObject("CSharpReferencesEvents");
                    csRefEvents.ReferenceAdded += new _dispReferencesEvents_ReferenceAddedEventHandler(HandleRefAdded);
                    csRefEvents.ReferenceRemoved += new _dispReferencesEvents_ReferenceRemovedEventHandler(HandleRefRemoved);
                    break;
                default:
                    var hierarchyItems = CLHelper.GetRefHierarchyItems(project);
                    foreach (var hierarchyItem in hierarchyItems)
                    {
                        uint cookie = 0;
                        hierarchyItem.AdviseHierarchyEvents(this.hierarchyEventsHelper, out cookie);
                        this.hierarchyEventsCookies.Add(cookie);
                    }
                    break;
            }
        }

        public void HandleRefAdded(VSLangProj.Reference pRef)
        {
            //triggered on project load for every direct and indirect dependency, ignore events for a few seconds after project load
            if(solutionLoadTimestamp < DateTime.Now.AddSeconds(-30))
            {
                refAddedHelper.Execute();
            }
        }

        public void HandleRefRemoved(VSLangProj.Reference pRef)
        {
            //TODO remove ref and CVEs if solely affected by ref or rescan?
        }

        private string GetCommandName(string Guid, int ID)
        {
            if (Guid == null)
                return "null";

            string result = "";
            if (commands != null)
            {
                try
                {
                    return commands.Item(Guid, ID).Name;
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        private void writeToErrorList(ConcurrentDictionary<int, Dependency> dependencies)
        {
            var errorListProvider = ErrorListHelper.ErrorListHelperInstance;
            errorListProvider.Tasks.Clear();
            foreach (var dependency in dependencies)
            {
                try
                {
                    int i = 0;
                    switch (dependency.Value.PolicyStatus)
                    {
                        case PolicyStatusEnum.Warn:
                            var t = new ErrorTask
                            {
                                ErrorCategory = TaskErrorCategory.Warning,
                                Category = TaskCategory.Misc,
                                Text = dependency.Value.GetErrorListDescription(),
                                Document = String.Join(", ", dependency.Value.IntroducedThroughFiles),
                            };
                            i = errorListProvider.Tasks.Add(t);
                            break;
                        case PolicyStatusEnum.Fail:
                            var t2 = new ErrorTask
                            {
                                ErrorCategory = TaskErrorCategory.Error,
                                Category = TaskCategory.Misc,
                                Text = dependency.Value.GetErrorListDescription(),
                                Document = String.Join(", ", dependency.Value.IntroducedThroughFiles),
                            };
                            i = errorListProvider.Tasks.Add(t2);
                            break;
                        case PolicyStatusEnum.Pass:
                        default:
                            continue;
                    }
                    errorListProvider.ForceShowErrors();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            Console.WriteLine(errorListProvider.Tasks.Count);
            //errorListProvider.ForceShowErrors();
        }
    }
}
