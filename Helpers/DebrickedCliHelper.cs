using Debricked.Models;
using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Debricked.Models.Constants;

namespace Debricked.Helpers
{
    internal static class DebrickedCliHelper
    {

        public static async Task updateDebrickedCliAsync(General settings)
        {
            await VS.StatusBar.ShowMessageAsync("Checking for Debricked CLI updates");
            GitHubRelease currentRelease = await getLatestDebrickedCliReleaseAsync();
            //setInstalledDebrickedCliVersion(store, "");
            //todo should the interval be a user setting?
            if (shouldUpdate(currentRelease, settings))
            {
                await VS.StatusBar.ShowMessageAsync("Installing new Debricked CLI version");
                await downloadAndInstallAsync(currentRelease, settings.DataDir, settings);
                SettingsHelper.SetInstalledDebrickedCliVersion(currentRelease.Version, settings);
                SettingsHelper.SetLastUpdateCheck(DateTime.Now, settings);
            }
        }

        private static async Task<GitHubRelease> getLatestDebrickedCliReleaseAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#user-agent-required");
                var release = await HttpHelper.MakeGetRequestAsync<GitHubRelease>(client, ApiEndpoints.DebrickedCliReleaseEndpoint, (e) => { });
                return release;
            }
        }


        private static bool shouldUpdate(GitHubRelease currentRelease, General settings)
        {
            if(currentRelease==null) return false;
            String installedVersion = SettingsHelper.GetInstalledDebrickedCliVersion(settings);
            if (string.IsNullOrEmpty(installedVersion))
            {
                return true;
            }
            else if (SettingsHelper.GetLastUpdateCheck(settings) < (DateTime.Now.AddDays(-1)))
            {
                SettingsHelper.SetLastUpdateCheck(DateTime.Now, settings);
                return new Version(currentRelease.Version).CompareTo(new Version(installedVersion)) > 0;
            }
            //todo change
            return false;
        }
        private static async Task downloadAndInstallAsync(GitHubRelease release, String appDataPath, General settings)
        {
            String assetPath = appDataPath + "/" + SettingsHelper.DebrickedAssetName;
            await downloadReleaseAsync(release, assetPath, settings);
            unpackRelease(assetPath, appDataPath);
        }

        private static async Task downloadReleaseAsync(GitHubRelease release, String assetPath, General settings)
        {
            HttpClientHandler handler = null;
            if (!string.IsNullOrEmpty(settings.Proxy))
            {
                handler = HttpHelper.GetHttpClientHandlerWithProxy(settings.Proxy, settings.IgnoreCertErrors);
            }
            using (HttpClient client = handler==null? new HttpClient() : new HttpClient(handler:handler))
            using (FileStream fs = File.Create(assetPath))
            {
                var assetStream = await client.GetStreamAsync(release.Assets.Where((a) => a.Name.Equals(SettingsHelper.DebrickedAssetName)).First().Url);
                await assetStream.CopyToAsync(fs);
            }
        }

        private static void unpackRelease(String assetPath, String targetPath)
        {
            using (var stream = File.OpenRead(assetPath))
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var memStream = new MemoryStream())
                {
                    //extract .gz
                    int read;
                    var buffer = new byte[4096];
                    do
                    {
                        read = gzip.Read(buffer, 0, buffer.Length);
                        memStream.Write(buffer, 0, read);
                    } while (read == buffer.Length);

                    //extract tar
                    memStream.Seek(0, SeekOrigin.Begin);
                    ExtractTar(memStream, targetPath);
                }
            }
        }

        private static void ExtractTar(Stream stream, string outputDir)
        {
            var buffer = new byte[100];
            while (true)
            {
                stream.Read(buffer, 0, 100);
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');
                if (String.IsNullOrWhiteSpace(name))
                    break;
                stream.Seek(24, SeekOrigin.Current);
                stream.Read(buffer, 0, 12);
                var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                stream.Seek(376L, SeekOrigin.Current);

                var output = Path.Combine(outputDir, name);
                if (!Directory.Exists(Path.GetDirectoryName(output)))
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!name.Equals("./", StringComparison.InvariantCulture))
                {
                    using (var str = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var buf = new byte[size];
                        stream.Read(buf, 0, buf.Length);
                        str.Write(buf, 0, buf.Length);
                    }
                }

                var pos = stream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);
            }
        }
    }
}
