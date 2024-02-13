using Debricked.Models.DebrickedApi;
using System.Windows;
using System.Windows.Controls;

namespace Debricked.Selectors
{
    internal class TreeViewRootItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DebrickedDependencyTreeNode node && container is FrameworkElement e)
            {
                if (node.Children.Count>0)
                {
                    return e.FindResource("StartIntervalTemplate") as DataTemplate;
                }
                else
                {
                    return e.FindResource("MidIntervalTemplate") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
