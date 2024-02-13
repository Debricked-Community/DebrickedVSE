using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal class HierarchyEventsHelper : IVsHierarchyEvents
    {
        private MasterEventHandlerHelper masterHandler;
        public HierarchyEventsHelper(MasterEventHandlerHelper masterEventHandlerHelper)
        {
            masterHandler = masterEventHandlerHelper;
        }

        public int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
        {
            masterHandler.HandleRefAdded(null);
            return VSConstants.S_OK;
        }

        public int OnItemsAppended(uint itemidParent)
        {
            masterHandler.HandleRefAdded(null);
            return VSConstants.S_OK;
        }

        public int OnItemDeleted(uint itemid)
        {
            masterHandler.HandleRefRemoved(null);
            return VSConstants.S_OK;
        }

        public int OnPropertyChanged(uint itemid, int propid, uint flags)
        {
            return VSConstants.S_OK;
        }

        public int OnInvalidateItems(uint itemidParent)
        {
            return VSConstants.S_OK;
        }

        public int OnInvalidateIcon(IntPtr hicon)
        {
            return VSConstants.S_OK;
        }
    }
}
