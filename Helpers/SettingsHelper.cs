using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using Debricked.Models.Constants;
using System.IO;
using Community.VisualStudio.Toolkit;

namespace Debricked.Helpers
{
    internal static class SettingsHelper
    {
        public static readonly String DebrickedAssetName = "cli_windows_x86_64.tar.gz";
        private static string DebrickedCliVersionFileName = "cliversion";
        private static string DebrickedCliLastUpdateFileName = "lastupdate";

        public static String GetInstalledDebrickedCliVersion(General settings)
        {
            var path = Path.Combine(settings.DataDir, DebrickedCliVersionFileName);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            } else 
            { 
                return string.Empty; 
            }
        }

        public static void SetInstalledDebrickedCliVersion(String version, General settings)
        {
            File.WriteAllText(Path.Combine(settings.DataDir, DebrickedCliVersionFileName), version);
        }

        public static DateTime GetLastUpdateCheck(General settings)
        {
            var path = Path.Combine(settings.DataDir, DebrickedCliLastUpdateFileName);
            if (File.Exists(path))
            {
                return DateTime.Parse(File.ReadAllText(path));
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public static void SetLastUpdateCheck(DateTime date, General settings)
        {
            File.WriteAllText(Path.Combine(settings.DataDir, DebrickedCliLastUpdateFileName), date.ToString());
        }

    }
}
