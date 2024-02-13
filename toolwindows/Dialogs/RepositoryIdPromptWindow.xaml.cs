using Community.VisualStudio.Toolkit;
using Debricked.Helpers;
using Debricked.Models;
using Debricked.Models.DebrickedApi;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace Debricked.toolwindows.Dialogs
{
    /// <summary>
    /// Interaction logic for RepositoryIdPromptWindow.xaml
    /// </summary>
    public partial class RepositoryIdPromptWindow
    {
        private string purposeString { get; set; }
        private General settings { get; set; }
        public DebrickedRepositoryIdentifier RepositoryIdentifier { get; set; } = null;
        private CollectionViewSource repos = new CollectionViewSource();
        public ObservableCollection<DebrickedRepositoryIdentifier> repoCollection = new ObservableCollection<DebrickedRepositoryIdentifier>();
        public RepositoryIdPromptWindow(Purpose purpose, General settings)
        {

            InitializeComponent();
            repos.Source = repoCollection;
            this.dgRepos.DataContext = repos.View;
            this.settings = settings;
            switch (purpose)
            {
                case Purpose.Map:
                    purposeString = "Please select the repository to which \nthe current Solution / Project should be mapped:";
                    break;
                case Purpose.CopyRules:
                    purposeString = "Please select the repository from which \nthe auto-created repository should copy its rules:";
                    break;
            }
            this.lblPurpose.Content = purposeString;

            using (DebrickedAPIHelper dApi = new DebrickedAPIHelper(settings, null))
            {
                foreach (var repo in ThreadHelper.JoinableTaskFactory.Run<List<DebrickedRepositoryIdentifier>>(() => dApi.GetRepositoriesAsync()))
                {
                    repoCollection.Add(repo);
                }
            }
            repos.View.Refresh();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            if(dgRepos.SelectedItem != null)
            {
                RepositoryIdentifier = (DebrickedRepositoryIdentifier)dgRepos.SelectedItem;
                this.Close();
            } else
            {
                VS.MessageBox.ShowError("Please select an entry in the list");
            }
        }
    }

    public enum Purpose
    {
        Map,
        CopyRules
    }
}
