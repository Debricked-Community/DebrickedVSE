using Microsoft.VisualStudio.PlatformUI;

namespace Debricked.toolwindows.Dialogs
{
    public class BaseDialogWindow : DialogWindow
    {
        public BaseDialogWindow()
        {
            this.HasMaximizeButton = true;
            this.HasMinimizeButton = true;
        }
    }
}
