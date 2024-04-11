using Community.VisualStudio.Toolkit;
using Debricked.Models;
using Debricked.Models.Constants;
using Debricked.Models.DebrickedApi;
using Debricked.toolwindows.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal class ScanHelper
    {
        private Solution solution;
        private Project project;
        private General settings;
        private ScanResult currentResult;
        private Dictionary<int, DebrickedRepositoryIdentifier> repositoryIdentifiers = new Dictionary<int, DebrickedRepositoryIdentifier>();

        public ScanHelper(Solution solution, Project project, General settings, ScanResult currentResult)
        {
            this.solution = solution;
            this.project = project;
            this.settings = settings;
            this.currentResult = currentResult;
        }
        public async Task<ScanResult> RunScanAsync()
        {
            try
            {
                string path = this.solution == null ? this.project.FullPath : this.solution.FullPath;
                string scopeName = this.solution == null ? this.project.Name : this.solution.Name;

                DebrickedRepositoryIdentifier mappedRepo = getMappedRepositoryIdentifier();
                DebrickedRepositoryIdentifier tempRepoIdentifier = null;

                if(settings.MappingStrategy == MappingStrategyEnum.Temporary)
                {
                    tempRepoIdentifier = await prepareTemporaryRepositoryAsync(Guid.NewGuid().ToString(), mappedRepo);
                }

                RunProcessResult result = await runScanAsync(path, scopeName, mappedRepo, tempRepoIdentifier);

                ScanResult sr = await finishScanAsync(result, mappedRepo, tempRepoIdentifier);
                sr.Solution = solution;
                return sr;

            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync("Debricked scan failed with exception: ", ex.ToString());
                return null;
            }
        }

        private async Task<RunProcessResult> runScanAsync(String scanPath, String solutionOrProjectName, DebrickedRepositoryIdentifier mappedRepositoryIdentifier, DebrickedRepositoryIdentifier tempRepoIdentifier)
        {
            await VS.StatusBar.ShowMessageAsync("Initializing Debricked scan");

            string cliPath = settings.GetDebrickedCliPath();
            string accessToken = await getAccessTokenAsync(settings);
            String projectRoot = Path.GetDirectoryName(scanPath);

            await runResolveAndFingerprintAsync(cliPath, projectRoot);

            StringBuilder scanArgsBeforeRepoName = new StringBuilder("scan ").Append(projectRoot)
                .Append(" -t ").Append(accessToken)
                .Append(" --no-resolve")
                .Append(" --json-path ").Append(Path.Combine(settings.DataDir, "result.json"));

            if (settings.Exclusions != null && settings.Exclusions.Count() > 0)
            {
                scanArgsBeforeRepoName.Append(" -e ").Append(String.Join(",", settings.Exclusions));
            }

            StringBuilder scanArgs = new StringBuilder(scanArgsBeforeRepoName.ToString());
            if(settings.MappingStrategy == MappingStrategyEnum.Temporary)
            {
                scanArgs.Append(" -r ").Append(tempRepoIdentifier.RepositoryName).Append(" -c ").Append(Guid.NewGuid());
            } 
            else if(settings.MappingStrategy == MappingStrategyEnum.Persistent)
            {
                if(settings.PersistentRepoStrategy == PersistentRepoStrategy.AlwaysAskRepoID)
                {
                    scanArgs.Append(" -r ").Append(mappedRepositoryIdentifier.RepositoryName).Append(" -c ").Append(Guid.NewGuid());
                }
            }


            await VS.StatusBar.ShowMessageAsync("Running scan");
            var scanResult = await ProcessHelper.RunProcessAsync(cliPath, scanArgs.ToString());
            if (scanResult.ExitCode != 0)
            {
                if (scanResult.StdErr.Contains("failed to find repository name") || scanResult.StdErr.Contains("failed to find commit hash"))
                {
                    scanArgs = new StringBuilder(scanArgsBeforeRepoName.ToString());
                    if(settings.MappingStrategy == MappingStrategyEnum.Persistent && settings.PersistentRepoStrategy == PersistentRepoStrategy.AutoDetectThenSolutionName)
                    {
                        scanArgs.Append(" -r ").Append(solutionOrProjectName).Append(" -c ").Append(Guid.NewGuid());
                        scanResult = await ProcessHelper.RunProcessAsync(cliPath, scanArgs.ToString());
                        scanResult.MappedRepository = new DebrickedRepositoryIdentifier { RepositoryName = solutionOrProjectName };
                    } else if(settings.MappingStrategy == MappingStrategyEnum.Persistent && settings.PersistentRepoStrategy == PersistentRepoStrategy.AutoDetectThenAskRepoID)
                    {
                        mappedRepositoryIdentifier = getRepoIdentifierFromPrompt(Purpose.Map);
                        scanArgs.Append(" -r ").Append(mappedRepositoryIdentifier.RepositoryName).Append(" -c ").Append(Guid.NewGuid());
                        scanResult = await ProcessHelper.RunProcessAsync(cliPath, scanArgs.ToString());
                        scanResult.MappedRepository = mappedRepositoryIdentifier;
                    } else
                    {
                        await clearStatusBarAndThrowAsync(scanResult.StdErr);
                    }
                }
                else
                {
                    if(string.IsNullOrEmpty(scanResult.StdErr) && scanResult.StdOut.Contains("fail pipeline")) 
                    {
                        scanResult.MappedRepository = mappedRepositoryIdentifier;
                    } else
                    {
                        await clearStatusBarAndThrowAsync(scanResult.StdErr);
                    }
                }
            } else
            {
                scanResult.MappedRepository = mappedRepositoryIdentifier;
            }

            await VS.StatusBar.ClearAsync();
            return scanResult;
        }

        private async Task<DebrickedRepositoryIdentifier> prepareTemporaryRepositoryAsync(string tempRepositoryName, DebrickedRepositoryIdentifier mappedRepositoryIdentifier)
        {
            try
            {
                using (DebrickedAPIHelper dApi = new DebrickedAPIHelper(settings, null))
                {
                    var tempRepoIdentifier = await dApi.CreateRepositoryAsync(tempRepositoryName);
                    if(mappedRepositoryIdentifier != null)
                    {
                        await dApi.CopyRepositoryRulesAsync(tempRepoIdentifier, mappedRepositoryIdentifier.RepositoryId);
                    }
                    return new DebrickedRepositoryIdentifier { RepositoryId = tempRepoIdentifier, RepositoryName = tempRepositoryName };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task removeTemporaryRepositoryAsync(int tempRepositoryId)
        {
            using (DebrickedAPIHelper dApi = new DebrickedAPIHelper(settings, null))
            {
                await dApi.DeleteRepositoryAsync(tempRepositoryId);
            }
        }

        private async Task runResolveAndFingerprintAsync(string cliPath, string projectRoot)
        {
            StringBuilder resolveArgs = new StringBuilder("resolve ").Append(projectRoot);
            if (settings.Exclusions != null && settings.Exclusions.Count() > 0)
            {
                resolveArgs.Append(" -e ").Append(String.Join(",", settings.Exclusions));
            }

            await VS.StatusBar.ShowMessageAsync("Resolving dependencies");

            RunProcessResult resolveResult = await ProcessHelper.RunProcessAsync(cliPath, resolveArgs.ToString());
            if (resolveResult.ExitCode != 0)
            {
                await clearStatusBarAndThrowAsync(resolveResult.StdErr);
            }

            if (settings.EnableFingerprinting)
            {
                StringBuilder fingerprintArgs = new StringBuilder("fingerprint ").Append(projectRoot);
                if (settings.Exclusions != null && settings.Exclusions.Count() > 0)
                {
                    fingerprintArgs.Append(" -e ").Append(String.Join(",", settings.Exclusions));
                }
                await ProcessHelper.RunProcessAsync(cliPath, fingerprintArgs.ToString());
            }
        }

        private static async Task<string> getAccessTokenAsync(General settings)
        {
            String accessToken = settings.getDecryptedDebrickedToken();
            if (String.IsNullOrEmpty(accessToken))
            {
                using (DebrickedAPIHelper apiHelper = new DebrickedAPIHelper(settings, null))
                {
                    await apiHelper.AuthenticateAsync();
                    accessToken = apiHelper.token.RefreshToken;
                }
            }

            return accessToken;
        }

        private async Task<ScanResult> finishScanAsync(RunProcessResult result, DebrickedRepositoryIdentifier mappedRepository, DebrickedRepositoryIdentifier tempRepoIdentifier)
        {
            if (result.ExitCode == 0 || result.StdErr.Length == 0)
            {
                string jsonPath = Path.Combine(settings.DataDir, "result.json");
                using (StreamReader reader = new StreamReader(jsonPath))
                using (DebrickedAPIHelper apiHelper = new DebrickedAPIHelper(settings, currentResult))
                {
                    string json = await reader.ReadToEndAsync();
                    DebrickedUploadResultReply uploadReply = JsonSerializer.Deserialize<DebrickedUploadResultReply>(json);
                    var ids = uploadReply.getRepoAndCommitId();
                    var scanResult = await apiHelper.getScanResultsAsync(ids.repositoryId, ids.commitId, uploadReply);
                    if(settings.MappingStrategy == MappingStrategyEnum.Temporary)
                    {
                        await removeTemporaryRepositoryAsync(tempRepoIdentifier.RepositoryId);
                        scanResult.MappingStrategy = MappingStrategyEnum.Temporary;
                        scanResult.MappedRepository = mappedRepository;
                    } else
                    {
                        scanResult.MappingStrategy = MappingStrategyEnum.Persistent;
                        scanResult.MappedRepository = null;
                    }
                    return scanResult;
                }
            }
            else
            {
                throw new Exception(result.StdErr);
            }
        }

        private DebrickedRepositoryIdentifier getMappedRepositoryIdentifier()
        {
            if(currentResult!=null && currentResult.MappedRepository!=null) return currentResult.MappedRepository;

            if (settings.MappingStrategy == MappingStrategyEnum.Temporary) 
            {
                if(settings.TempRepoStrategy == TempRepoStrategy.NoMapping)
                {
                    return null;
                } else if(settings.TempRepoStrategy == TempRepoStrategy.AlwaysAskRepoID)
                {
                    return getRepoIdentifierFromPrompt(Purpose.CopyRules);
                }
                else
                {
                    throw new Exception("unknown TempRepoStrategy");
                }
            } else if(settings.MappingStrategy == MappingStrategyEnum.Persistent)
            {
                if(settings.PersistentRepoStrategy == PersistentRepoStrategy.AutoDetectThenSolutionName || settings.PersistentRepoStrategy == PersistentRepoStrategy.AutoDetectThenAskRepoID)
                {
                    return null;
                } else if(settings.PersistentRepoStrategy == PersistentRepoStrategy.AlwaysAskRepoID)
                {
                    return getRepoIdentifierFromPrompt(Purpose.CopyRules);
                }
                else
                {
                    throw new Exception("unknown PersistentRepoStrategy");
                }
            } else
            {
                throw new Exception("unknown mapping strategy");
            }
        }

        private DebrickedRepositoryIdentifier getRepoIdentifierFromPrompt(Purpose purpose)
        {
            RepositoryIdPromptWindow prompt = new RepositoryIdPromptWindow(purpose, settings);
            prompt.ShowDialog();
            if(prompt.RepositoryIdentifier==null)
            {
                throw new Exception("Unable to get repository identifier for mapping");
            } else
            {
                this.repositoryIdentifiers = prompt.repoCollection.ToDictionary(x => x.RepositoryId);
                return prompt.RepositoryIdentifier;
            }

        }

        private async Task clearStatusBarAndThrowAsync(string message)
        {
            await VS.StatusBar.ClearAsync();
            throw new Exception(message);
        }
    }
}
