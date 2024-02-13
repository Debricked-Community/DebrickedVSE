using Community.VisualStudio.Toolkit;
using Debricked.Helpers;
using Debricked.Models.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Debricked
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("Authentication")]
        [DisplayName("Debricked Token")]
        [Description("Debricked Token for authentication, retrieve from Admin tools page or your company administrator. Optional if Username and Password are provided.")]
        [PasswordPropertyText(true)]
        public String DebrickedToken { get; set; } = "";

        [Category("Authentication")]
        [DisplayName("Debricked Username")]
        [Description("Your Debricked username. Optional if Debricked Token is provided")]
        public String Username { get; set; } = "";

        [Category("Authentication")]
        [DisplayName("Debricked Password")]
        [Description("Your Debricked password. Optional if Debricked Token is provided")]
        [PasswordPropertyText(true)]
        public String Password { get; set; } = "";

        [Category("Scan settings")]
        [DisplayName("Scan entire solution")]
        [Description("If enabled the entire solution (all projects) is scanned. If disabled only the active project is scanned.")]
        public bool ScanSolution { get; set; } = true;

        [Category("Scan settings")]
        [DisplayName("Exclusions")]
        [Description("Items to exclude during resolve and scan actions. ")]
        public String[] Exclusions { get; set; }

        [Category("Scan settings")]
        [DisplayName("Enable Fingerprinting")]
        [Description("If enabled \"debricked fingerprint\" will be run in addition to \"debricked resolve\"")]
        public bool EnableFingerprinting { get; set; }
        
        [Category("Scan settings")]
        [DisplayName("Data refresh interval")]
        [Description("Number of hours until a full data refresh is done. Until the timeout is reached the extension incrementally updates collected data to reduce the number of API calls.")]
        public int DataRefreshInterval { get; set; } = 24;

        [Category("Scan settings")]
        [DisplayName("Repository type")]
        [Description("Persistent: Uses a persistent repository \nTemporary: Uses a repository that will be deleted automatically after each scan")]
        public MappingStrategyEnum MappingStrategy { get; set; } = MappingStrategyEnum.Persistent;

        [Category("Scan settings")]
        [DisplayName("Persistent repository mapping strategy")]
        [Description("Choose how the extension maps a Solution/Project to a persistent Debricked repository. Mapping happens on the initial scan and is then persisted.\nAutoDetectThenSolutionName: Trys to autodetect repository name, falls back to the Solution/Project name \nAutoDetectThenAskRepoID: Trys to autodetect repository name, ask the user for a repository ID as fallback \nAlwaysAskRepoID: Always ask the user for a repository ID")]
        public PersistentRepoStrategy PersistentRepoStrategy { get; set; } = PersistentRepoStrategy.AutoDetectThenSolutionName;

        [Category("Scan settings")]
        [DisplayName("Temporary repository mapping strategy")]
        [Description("Choose how the extension copies rules of a persistent repository to it´s temporary repository. Mapping happens on the initial scan and is then persisted.\nAlwaysAskRepoID: Always ask the user for a repository ID \nNoMapping: No mapping is applied WARNING: policy checks will only be executed against default rules")]
        public TempRepoStrategy TempRepoStrategy { get; set; } = TempRepoStrategy.AlwaysAskRepoID;


        //repository mapping
        //persistent repo:
        //autodetect then use solution/proj name
        //autodetect then ask for repoId
        //allways ask for repoid
        //temp repo
        //ask for repoId

        [Category("General")]
        [DisplayName("Data Directory")]
        [Description("The directory used to store the Debricked CLI executable and scan results")]
        [DefaultValue("")]
        public string DataDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DebrickedVSe");

        [Category("General")]
        [DisplayName("Automatically update Debricked CLI")]
        [Description("Automatically check for and update to new versions of the Debricked CLI.")]
        [DefaultValue(true)]
        public bool AutoUpdate { get; set; } = true;

        [Category("General")]
        [DisplayName("Interval for automatic updates")]
        [Description("Number of hours between update checks")]
        [DefaultValue(24)]
        public int AutoUpdateInterval { get; set; } = 24;

        [Category("Connection")]
        [DisplayName("Proxy")]
        [Description("Url of a proxy to use, format: http(s)://<proxy-IP or DNS name>:<proxy-port>")]
        [DefaultValue("")]
        public string Proxy { get; set; } = "";

        [Category("Connection")]
        [DisplayName("Ignore certificate errors")]
        [Description("When enabled SSL certificate validation errors will be ignored")]
        [DefaultValue(false)]
        public bool IgnoreCertErrors { get; set; } = false;


        [Category("Triggers")]
        [DisplayName("On reference added")]
        [Description("Triggers a rescan when a reference is added (supported for C#, VB)")]
        public bool Trigger_OnReferenceAdded { get; set; } = true;

        [Category("Triggers")]
        [DisplayName("After build (unless debugging)")]
        [Description("Triggers a rescan after the Solution or Project is built, unless the build is followed by a debug session")]
        public bool Trigger_OnBuild { get; set; } = true;

        [Category("Triggers")]
        [DisplayName("After build (always)")]
        [Description("Triggers a rescan after the Solution or Project is built, regardless of the build being followed by a debug session")]
        public bool Trigger_OnBuildWithDebug { get; set; } = false;

        public override void Save()
        {
            if (!String.IsNullOrEmpty(this.DebrickedToken) && !this.DebrickedToken.EndsWith("enc"))
            {
                this.DebrickedToken = EncryptionHelper.Encrypt(this.DebrickedToken) + "enc";
            }
            if (!String.IsNullOrEmpty(this.Password) && !this.Password.EndsWith("enc"))
            {
                this.Password = EncryptionHelper.Encrypt(this.Password) + "enc";
            }
            try
            {
                var di = new DirectoryInfo(this.DataDir);
                if(!di.Exists)
                {
                    Directory.CreateDirectory(this.DataDir);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Datadir path is invalid", ex);
            }

            base.Save();

        }

        public String getDecryptedDebrickedToken()
        {
            return String.IsNullOrEmpty(this.DebrickedToken) ? "" : EncryptionHelper.Decrypt(this.DebrickedToken.Substring(0, this.DebrickedToken.Length - 3));
        }

        public String getDecryptedDebrickedPassword()
        {
            return String.IsNullOrEmpty(this.Password) ? "" : EncryptionHelper.Decrypt(this.Password.Substring(0, this.Password.Length - 3));
        }

        public string GetDebrickedCliPath()
        {
            return Path.Combine(this.DataDir, "debricked.exe");
        }
    }
}
