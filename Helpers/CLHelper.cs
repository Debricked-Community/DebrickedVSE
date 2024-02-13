using Community.VisualStudio.Toolkit;
using Debricked.Models.Constants;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal static class CLHelper
    {
        public static CLProjectTypeEnum GetProjectType(Project project)
        {
            if(project.FullPath.EndsWith("csproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.CSProj;
            }
            else if(project.FullPath.EndsWith("pyproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.PyProj;
            }
            else if (project.FullPath.EndsWith("njsproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.NjsProj;
            }
            else if (project.FullPath.EndsWith("esproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.EsProj;
            }
            else if (project.FullPath.EndsWith("fsproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.FsProj;
            }
            else if (project.FullPath.EndsWith("dcproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.DcProj;
            }
            else if (project.FullPath.EndsWith("vbproj", StringComparison.OrdinalIgnoreCase))
            {
                return CLProjectTypeEnum.VbProj;
            }
            return CLProjectTypeEnum.Unknown;
        }

        public static List<IVsHierarchy> GetRefHierarchyItems(Project project)
        {
            List<IVsHierarchy> result = new List<IVsHierarchy>();
            switch (GetProjectType(project))
            {
                case CLProjectTypeEnum.PyProj:
                    {
                        IVsHierarchy item = null;
                        foreach (var solutionItem in project.Children)
                        {
                            //virtual folder "Python Environments", name is localized but seems to be starting with python (at least in en-us and de-de)
                            if (solutionItem.Type == SolutionItemType.VirtualFolder && solutionItem.Text.StartsWith("python", StringComparison.OrdinalIgnoreCase))
                            {
                                //there is a subfolder containing dependency entries for each environment added to the project
                                foreach (var pythonEnvironment in solutionItem.Children)
                                {
                                    pythonEnvironment.GetItemInfo(out item, out _, out _);
                                    result.Add(item);
                                }
                                break;
                            }
                        }
                        break;
                    }

                case CLProjectTypeEnum.CSProj:
                    break;
                case CLProjectTypeEnum.FsProj:
                    break;
                case CLProjectTypeEnum.VbProj:
                    break;
                case CLProjectTypeEnum.DcProj:
                    break;
                case CLProjectTypeEnum.NjsProj:
                    break;
                case CLProjectTypeEnum.EsProj:
                    break;
                case CLProjectTypeEnum.Unknown:
                    default: break;
            }
            return result;
        }
    }
}
