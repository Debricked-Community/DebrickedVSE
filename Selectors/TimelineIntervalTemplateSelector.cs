using Debricked.Models.DebrickedApi;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Debricked.Selectors
{
    internal class TimelineIntervalTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MyProperty { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is DebrickedVulnTimeLineInterval interval && container is FrameworkElement e)
            {
                if (!String.IsNullOrEmpty(interval.StartVersion) && interval.StartVersion.Equals("zero"))
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
