using Debricked.Models.Constants;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal class ErrorListHelper
    {
        private static ErrorListHelper instance;
        private ErrorListProvider provider;

        private ErrorListHelper()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            provider = new ErrorListProvider(ServiceProvider.GlobalProvider);
            provider.ProviderName = "Debricked";
            provider.ProviderGuid = Guids.ErrorListProviderGuid;
        }

        public static ErrorListProvider ErrorListHelperInstance { get { 
                if(instance == null) { instance = new ErrorListHelper(); }
                return instance.provider; 
            } }
    }
}
