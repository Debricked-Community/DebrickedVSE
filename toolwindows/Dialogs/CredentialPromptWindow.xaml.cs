using Community.VisualStudio.Toolkit;
using Debricked.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Debricked.toolwindows.Dialogs
{
    /// <summary>
    /// Interaction logic for CredentialPromptWindow.xaml
    /// </summary>
    public partial class CredentialPromptWindow
    {
        public bool Authenticated { get; private set; } = false;

        private General settings { get; set; }

        public CredentialPromptWindow(General settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            bool unpwset = false, tset = false;
            if(this.tb_Username.Text != "" && this.tb_Password.Password != "")
            {
                unpwset = true;
                try
                {
                    using (var dApi = new DebrickedAPIHelper(settings, null))
                    {
                        if(ThreadHelper.JoinableTaskFactory.Run<bool>(async () => await dApi.AuthenticateAsync(this.tb_Username.Text, this.tb_Password.Password)))
                        {
                            Authenticated = true;
                            this.settings.Username = this.tb_Username.Text;
                            this.settings.Password = this.tb_Password.Password;
                            this.settings.Save();
                            this.Close();
                        } else
                        {
                            VS.MessageBox.ShowError("Username or password is wrong");
                        }
                    }
                }
                catch (Exception ex)
                {
                    VS.MessageBox.ShowError("Exception while verifying credentials: " + ex.ToString());
                }
            }
            if (this.tb_Token.Password != "")
            {
                tset = true;
                try
                {
                    using (var dApi = new DebrickedAPIHelper(settings, null))
                    {
                        if (ThreadHelper.JoinableTaskFactory.Run<bool>(async () => await dApi.AuthenticateAsync(this.tb_Token.Password)))
                        {
                            Authenticated = true;
                            this.settings.DebrickedToken = this.tb_Token.Password;
                            this.settings.Save();
                            this.Close();
                        }
                        else
                        {
                            VS.MessageBox.ShowError("Token is wrong");
                        }
                    }
                }
                catch (Exception ex)
                {
                    VS.MessageBox.ShowError("Exception while verifying token: " + ex.ToString());
                }
            }
            if(!tset && !unpwset)
            {
                VS.MessageBox.ShowError("Please enter values for Username and Password or Token");
            }
        }
    }
}
