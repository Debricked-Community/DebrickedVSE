﻿using Community.VisualStudio.Toolkit;
using Debricked.Helpers;
using Debricked.Models.Constants;
using Debricked.Models;
using Debricked.Models.DebrickedApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Debricked.toolwindows.main.Controls
{
    /// <summary>
    /// Interaction logic for DG_Vulnerabilities.xaml
    /// </summary>
    public partial class DG_Vulnerabilities : UserControl
    {
        public DG_Vulnerabilities()
        {
            InitializeComponent();
        }

        private int previouslySelectedDgIndex = -1;
        private void dgVulns_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //allow closing of the details section when an item is clicked repeatedly
            if (sender is DataGrid dataGrid)
            {
                if (e.OriginalSource is Image image)
                {
                    linkImage_MouseLeftButtonUp(e.OriginalSource, e);
                }
                if (dataGrid.RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.VisibleWhenSelected && dataGrid.SelectedIndex == previouslySelectedDgIndex)
                {
                    //do not collapse if the user clicks on the detail pane
                    if (e.OriginalSource is FrameworkElement uie && uie.Parent != null && uie.Parent is DataGridCell)
                    {
                        dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                    }
                }
                else
                {
                    previouslySelectedDgIndex = dataGrid.SelectedIndex;
                    dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                }
            }
        }

        private void icTrees_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView treeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void linkImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image)
            {
                if (image.DataContext is DebrickedVulnRefSummary refSummary && !String.IsNullOrEmpty(refSummary.Link))
                {
                    Process.Start(refSummary.Link);
                }
                else
                {
                    image.IsEnabled = false;
                }
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && !e.Handled)
            {
                if (scrollViewer.ActualHeight < scrollViewer.MaxHeight || (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight && e.Delta < 0) || (scrollViewer.VerticalOffset == 0 && e.Delta > 0))
                {
                    e.Handled = true;
                    var scv = ScrollViewerHelper.GetScrollViewer(dgVulns);
                    if (scv != null)
                    {
                        var d = e.Delta / 3; //row height is 25, dg scrollviewer scrolls by 48
                        //delta is always 120 which leads to a huge jump
                        //if the targets scrollviewer CanContentScroll property is true the offset is in items rather than dpi
                        //there is no "in-between" items so if rowDetails is open, consumes the rest of the view and an offset is applied it will skip the rowdetails entirely and jump to the next itme
                        //it might be possible to force the grids scrollviwer into physical scrolling (by DPI) 
                        //BUT physical scrolling requires fully measuring all items in the view to determine scroll length
                        //this is apparently very bad for performance with larger datasets, also unclear if opening RowDetails will trigger a re-measurement -> probable
                        scv.ScrollToVerticalOffset(scv.VerticalOffset - d);
                    }
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if(sender is  Hyperlink link && link.NavigateUri!=null && !string.IsNullOrEmpty(link.NavigateUri.ToString()))
            {
                Process.Start(link.NavigateUri.ToString());
            }
        }
    }
}
