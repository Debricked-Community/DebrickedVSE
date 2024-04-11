using Community.VisualStudio.Toolkit;
using Debricked.Extensions;
using Debricked.Models;
using Debricked.Models.Constants;
using Debricked.Models.DebrickedApi;
using Debricked.toolwindows.Dialogs;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal class DebrickedAPIHelper : IDisposable
    {
        private HttpClient client;
        public DebrickedTokenReply token { get; private set; } = null;
        private General settings = null;
        private bool authenticated = false;
        private ScanResult scanResult = null;
        private readonly Regex vulnIdRegex = new Regex(RegEx.FirstIdInLinkRegex, RegexOptions.Compiled);
        private bool depsChanged = false;

        public DebrickedAPIHelper(General settings, ScanResult currentResult)
        {
            this.settings = settings;
            if (settings != null && !string.IsNullOrEmpty(settings.Proxy))
            {
                this.client = new HttpClient(handler: HttpHelper.GetHttpClientHandlerWithProxy(settings.Proxy, settings.IgnoreCertErrors));
            } else
            {
                this.client = new HttpClient();
            }
            this.client.BaseAddress = new Uri(ApiEndpoints.DebrickedBaseUrl);
            this.scanResult = currentResult;
            ThreadHelper.JoinableTaskFactory.Run(AuthenticateAsync);
        }

        public async Task<ScanResult> getScanResultsAsync(int repositoryId, int commitId, DebrickedUploadResultReply uploadResultReply)
        {
            await VS.StatusBar.ShowMessageAsync("Fetching Debricked results");

            //dependencyId, ruleId, subject, values
            Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping = ConversionHelper.DebrickedUploadResultReplyToDict(uploadResultReply);

            if (scanResult != null && scanResult.LastFullRefresh.AddHours(settings.DataRefreshInterval) < DateTime.Now)
            {
                return await getUpdateResultsAsync(repositoryId, commitId, depPolicyMapping);
            }
            else
            {
                var depTask = getDependenciesAsync(repositoryId, commitId);
                var vulnTask = getVulnerabilitiesAsync(repositoryId, commitId);
                var licTask = getEntitiesAsync<DebrickedLicensesReply, DebrickedLicense>(repositoryId, commitId, ApiEndpoints.LicensesUrl, failOnError: false);
                //TODO add exception handler
                var rulesTask = HttpHelper.MakePostRequestAsync<DebrickedRulesReply>(client, String.Format(ApiEndpoints.RulesUrl, repositoryId), new StringContent(""));

                await Task.WhenAll(depTask, licTask, vulnTask, rulesTask);
                var rules = ConversionHelper.DebrickedRuleGroupListToRuleDict((await rulesTask).RuleGroups, repositoryId);

                return new ScanResult(repositoryId, commitId,
                    ConversionHelper.DebrickedVulnListToVulnDict(await vulnTask),
                    ConversionHelper.DebrickedDepListToDepDict(await depTask, rules),
                    ConversionHelper.DebrickedLicListToLicDict(await licTask),
                    rules,
                    depPolicyMapping);
            }

        }

        private async Task<ScanResult> getUpdateResultsAsync(int repositoryId, int commitId, Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping)
        {
            var licTask = getEntitiesAsync<DebrickedLicensesReply, DebrickedLicense>(repositoryId, commitId, ApiEndpoints.LicensesUrl, failOnError: false);
            var depTask = getEntitiesAsync<DebrickedDependenciesReply, DebrickedDependency>(repositoryId, commitId, ApiEndpoints.DependenciesUrl, true);
            var vulnTask = getEntitiesAsync<DebrickedVulnerabilitiesReply, DebrickedVulnerability>(repositoryId, commitId, ApiEndpoints.VulnerabilitiesUrl, failOnError: true);
            var rulesTask = HttpHelper.MakePostRequestAsync<DebrickedRulesReply>(client, String.Format(ApiEndpoints.RulesUrl, repositoryId), new StringContent(""));

            await Task.WhenAll(depTask, vulnTask, rulesTask, licTask);

            //TODO compare rules and update all policy results if diff

            DebrickedRulesReply rules = await rulesTask;
            var ruleIds = rules.RuleGroups.Select(g => g.GetRuleId(repositoryId));

            var removedRuleIds = scanResult.Rules.Keys.Except(ruleIds);
            var newRuleIds = ruleIds.Except(scanResult.Rules.Keys);
            this.scanResult.Rules = ConversionHelper.DebrickedRuleGroupListToRuleDict((await rulesTask).RuleGroups, repositoryId);

            Dictionary<int, DebrickedDependency> dependencies = (await depTask).ToDictionary((x) => x.Id);

            var removedDepIds = scanResult.Dependencies.Keys.Except(dependencies.Keys);
            var newDepIds = dependencies.Keys.Except(scanResult.Dependencies.Keys);
            var updateDepTask = updateDependenciesAsync(repositoryId, commitId, removedDepIds, newDepIds, dependencies);
            if (removedDepIds.Count() != 0 || newDepIds.Count() != 0) depsChanged = true;

            Dictionary<int,DebrickedVulnerability> vulnerabilities = (await vulnTask).Where(x => x.VulnerabilityStatus!=VulnerabilityStatusEnum.Unaffected).ToDictionary((x) => x.GetId(this.vulnIdRegex));
            Task<Dictionary<int, DebrickedVulnerability>> updateVulnTask = null;

            var removedVulnIds = scanResult.Vulnerabilities.Keys.Except(vulnerabilities.Keys);
            var newVulnIds = vulnerabilities.Keys.Except(scanResult.Vulnerabilities.Keys);
            if(removedVulnIds.Count()!=0 || newVulnIds.Count() != 0)
            {
                updateVulnTask = updateVulnerabilitiesAsync(repositoryId, commitId, removedVulnIds, newVulnIds, vulnerabilities);
            }
            
            await Task.WhenAll(new Task[] { updateDepTask, updateVulnTask }.Where(i => i != null));

            Dictionary<int, Vulnerability> vulns = new Dictionary<int, Vulnerability>();
            if (updateVulnTask != null) vulns = ConversionHelper.DebrickedVulnListToVulnDict((await updateVulnTask).Values.ToList());


            scanResult.MergeNewVulnerabilitiesAndDependencies(vulns,
                ConversionHelper.DebrickedDepListToDepDict((await updateDepTask).Values.ToList(), scanResult.Rules),
                ConversionHelper.DebrickedLicListToLicDict(await licTask), commitId, depPolicyMapping);
            return this.scanResult;

        }

        private async Task<Dictionary<int, DebrickedVulnerability>> updateVulnerabilitiesAsync(int repositoryId, int commitId, IEnumerable<int> removedVulnIds, IEnumerable<int> newVulnIds, Dictionary<int, DebrickedVulnerability> vulnerabilities)
        {
            var removeVulnsTask = removedVulnIds.ParallelForeachAsync(async vulnId => { this.scanResult.Vulnerabilities.TryRemove(vulnId, out _); this.scanResult.VulnToDepMapping.TryRemove(vulnId, out _); }, Environment.ProcessorCount);
            var updateVulnsTask = vulnerabilities.ParallelForeachAsync(async (vulnerability) =>
            {
                //TODO 
                if (depsChanged)
                {
                    vulnerabilities[vulnerability.Key] = await getVulnerabilityDetailsAsync(vulnerability.Value, repositoryId, commitId);
                } else if(newVulnIds.Contains(vulnerability.Key))
                {
                    vulnerabilities[vulnerability.Key] = await getVulnerabilityDetailsAsync(vulnerability.Value, repositoryId, commitId);
                }
            }, Environment.ProcessorCount);

            await Task.WhenAll(removeVulnsTask, updateVulnsTask);
            return vulnerabilities;
        }

        private async Task<Dictionary<int, DebrickedDependency>> updateDependenciesAsync(int repositoryId, int commitId, IEnumerable<int> removedDepIds, IEnumerable<int> newDepIds, Dictionary<int, DebrickedDependency> dependencies)
        {
            var removeDepsTask = removedDepIds.ParallelForeachAsync(async depId => { this.scanResult.Dependencies.TryRemove(depId, out _); this.scanResult.DependencyIds.TryRemove(depId, out _); }, Environment.ProcessorCount);
            var updateDepsTask = dependencies.ParallelForeachAsync(async dependency =>
            {
                if (newDepIds.Contains(dependency.Key))
                {
                    dependencies[dependency.Key] = await getDependencyDetailsAsync(dependency.Value, repositoryId, commitId);

                } else
                {
                    dependencies[dependency.Key] = await getDependencyFilesAndPolicyStatusAsync(dependency.Value, repositoryId, commitId);
                    
                }
            }, Environment.ProcessorCount);

            await Task.WhenAll(removeDepsTask, updateDepsTask);
            return dependencies;
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }

        private async Task<List<DebrickedVulnerability>> getVulnerabilitiesAsync(int repositoryId, int commitId)
        {
            List<DebrickedVulnerability> vulns = await getEntitiesAsync<DebrickedVulnerabilitiesReply, DebrickedVulnerability>(repositoryId, commitId, ApiEndpoints.VulnerabilitiesUrl, failOnError:true);
            vulns = vulns.Where(x => x.VulnerabilityStatus != VulnerabilityStatusEnum.Unaffected).ToList();
            await vulns.ParallelForeachAsync<DebrickedVulnerability>(async(vulnerability)  =>
            {
                vulnerability = await getVulnerabilityDetailsAsync(vulnerability, repositoryId, commitId);
            }, Environment.ProcessorCount);
            return vulns;
        }

        private async Task<List<DebrickedDependency>> getDependenciesAsync(int repositoryId, int commitId)
        {
            List<DebrickedDependency> dependencies = await getEntitiesAsync<DebrickedDependenciesReply, DebrickedDependency>(repositoryId, commitId, ApiEndpoints.DependenciesUrl, true);
            await dependencies.ParallelForeachAsync(async (dependency) =>
            {
                dependency = await getDependencyDetailsAsync(dependency, repositoryId, commitId);

            }, Environment.ProcessorCount);
            return dependencies;
        }

        private async Task<DebrickedVulnerability> getVulnerabilityDetailsAsync(DebrickedVulnerability vulnerability, int repositoryId, int commitId)
        {
            //pull root fix info and files (introduced through)
            vulnerability.Id = vulnerability.GetId(vulnIdRegex);
            var rootFixTask = HttpHelper.MakeGetRequestAsync<DebrickedRootFixReply>(client, String.Format(ApiEndpoints.RootFixUrl, vulnerability.Id, repositoryId, commitId), (e) => HandleWebException(e, false));
            var filesTask = HttpHelper.MakeGetRequestAsync<List<DebrickedFileReference>>(client, String.Format(ApiEndpoints.VulnerabilityFilesUrl, vulnerability.Id, commitId), (e) => HandleWebException(e, false));
            var timeLineTask = HttpHelper.MakeGetRequestAsync<DebrickedVulnTimelineReply>(client, String.Format(ApiEndpoints.VulnTimeLineUrl, vulnerability.Id, repositoryId), (e) => HandleWebException(e, false));
            vulnerability.Link = ApiEndpoints.DebrickedBaseUrl + vulnerability.Link;

            List<DebrickedFileReference> files = await filesTask;

            List<DebrickedDependencyTreeReply> treeReplys = new List<DebrickedDependencyTreeReply>();
            if (files != null)
            {
                foreach (var file in files)
                {
                    var treeReply = await HttpHelper.MakeGetRequestAsync<DebrickedDependencyTreeReply>(client, String.Format(ApiEndpoints.FixDependencyTreeUrl, vulnerability.Id, file.Id, commitId), (e) => HandleWebException(e, false));
                    if (treeReply != null)
                    {
                        treeReplys.Add(treeReply);
                    }
                }
            }

            DebrickedRootFixReply rootFixes = await rootFixTask;
            if (rootFixes != null)
            {
                vulnerability.IsRootFixAvailable = rootFixes.RootFixesCount > 0;
                vulnerability.IsDirectFixAvailable = rootFixes.RootFixes.Count > 0;
                foreach (DebrickedDependencyTreeReply treeReply in treeReplys)
                {
                    for (int i = 0; i < treeReply.Trees.Count; i++)
                    {
                        treeReply.Trees[i] = getRootFixInfoRecursive(treeReply.Trees[i], rootFixes, treeReply.Name);
                        vulnerability.Trees.Add(treeReply.Trees[i]);
                    }
                }
            }

            //add timeline info
            DebrickedVulnTimelineReply vulnTimelineReply = await timeLineTask;
            if (vulnTimelineReply != null && vulnTimelineReply.Timelines != null)
            {
                foreach (DebrickedVulnTimeLineInnerReply timeLineInnerReply in vulnTimelineReply.Timelines)
                {
                    foreach (var dependencyName in timeLineInnerReply.DependencyNames)
                    {
                        vulnerability.VulnerabilityTimelineIntervals.Add(dependencyName.Name, timeLineInnerReply.Intervals.ToHashSet());
                    }
                }
            }

            //TODO add referencesummaries
            DebrickedVulnRefSummaryReply refSummary = await HttpHelper.MakeGetRequestAsync<DebrickedVulnRefSummaryReply>(client, String.Format(ApiEndpoints.RefSummaryUrl, vulnerability.Id), (e) => HandleWebException(e, false));
            if (refSummary != null)
            {
                vulnerability.RefSummaries = refSummary.GetPresentAsList();
            }
            return vulnerability;
        }

        private async Task<DebrickedDependency> getDependencyDetailsAsync(DebrickedDependency dependency, int repositoryId, int commitId)
        {
            dependency.Link = ApiEndpoints.DebrickedBaseUrl + dependency.Link;
            DebrickedDependencyInfoReply infoReply = await HttpHelper.MakeGetRequestAsync<DebrickedDependencyInfoReply>(client, String.Format(ApiEndpoints.DependencyInfoUrl, dependency.Id), (e) => HandleWebException(e, false));
            if(infoReply is null)
            {
                //try to resolve select id through package url
                DebrickedSelectDependencyDataReply ddata = await HttpHelper.MakeGetRequestAsync<DebrickedSelectDependencyDataReply>(client, String.Format(ApiEndpoints.SelectDataUrl, dependency.Id), (e) => HandleWebException(e, false));
                if (ddata != null)
                {
                    DebrickedGetSelectDependencyIdReply selectIdReply = await HttpHelper.MakeGetRequestAsync<DebrickedGetSelectDependencyIdReply>(client, String.Format(ApiEndpoints.GetSelectIdUrl, ddata.GetPurl()), (e) => HandleWebException(e, false));
                    if (selectIdReply != null) dependency.SelectId = selectIdReply.DependencyId;
                }
            } else
            {
                dependency.SelectId = infoReply.SelectId;
            }

            dependency = await getDependencyFilesAndPolicyStatusAsync(dependency, repositoryId, commitId);

            if (dependency.SelectId != -1)
            {
                DebrickedOSHDataReply oshdata = await HttpHelper.MakeGetRequestAsync<DebrickedOSHDataReply>(client, String.Format(ApiEndpoints.OshDataUrl, dependency.SelectId), (e) => HandleWebException(e, false));
                if (oshdata != null && oshdata.Metrics != null)
                {
                    dependency.PopularityScore = oshdata.Metrics.PopularityScore != null ? oshdata.Metrics.PopularityScore.Score / 10 : -1;
                    dependency.ContributorsScore = oshdata.Metrics.ContributorsScore != null ? oshdata.Metrics.ContributorsScore.Score / 10 : -1;
                    dependency.SecurityScore = oshdata.Metrics.SecurityScore != null ? oshdata.Metrics.SecurityScore.Score / 10 : -1;
                }
            }
            return dependency;
        }

        private async Task<DebrickedDependency> getDependencyFilesAndPolicyStatusAsync(DebrickedDependency dependency, int repositoryId, int commitId)
        {
            var introducedThroughFilesTask = HttpHelper.MakeGetRequestAsync<List<DebrickedFileReference>>(client, String.Format(ApiEndpoints.DependencyFilesUrl, dependency.Id, repositoryId, commitId), (e) => HandleWebException(e, false));
            var policyDataTask = dependency.SelectId!=-1 ? HttpHelper.MakeGetRequestAsync<DebrickedPolicyHintsReply>(client, String.Format(ApiEndpoints.PolicyHintsUrl, dependency.SelectId, repositoryId), (e) => HandleWebException(e, false)) : null;

            List<DebrickedFileReference> introducedTroughFiles = await introducedThroughFilesTask;
            dependency.IntroducedThroughFiles = introducedTroughFiles.Select((x) => x.Name).ToList();

            if (dependency.SelectId != -1)
            {
                DebrickedPolicyHintsReply policyData = await policyDataTask;
                if (policyData != null && policyData.Summary != null)
                {
                    if (policyData.Summary.Failure.Count > 0)
                    {
                        dependency.PolicyStatus = PolicyStatusEnum.Fail;
                    }
                    else if (policyData.Summary.Warning.Count > 0)
                    {
                        dependency.PolicyStatus = PolicyStatusEnum.Warn;
                    }
                    else
                    {
                        dependency.PolicyStatus = PolicyStatusEnum.Pass;
                    }
                    dependency.PolicyData = policyData;
                }
            }
            return dependency;
        }

        private DebrickedDependencyTreeNode getRootFixInfoRecursive(DebrickedDependencyTreeNode treeNode, DebrickedRootFixReply rootFixes, String introducedThrough)
        {
            treeNode.IntroducedThrough = introducedThrough;

            StringBuilder rootFixKey = new StringBuilder(treeNode.Name).Append("#").Append(treeNode.Version);
            if (rootFixes.RootFixes.ContainsKey(rootFixKey.ToString()))
            {
                treeNode.Fix = rootFixes.RootFixes[rootFixKey.ToString()] == "unknown" ? "" : rootFixes.RootFixes[rootFixKey.ToString()];
            }

            if(!(treeNode.Children is null) && treeNode.Children.Count > 0)
            {
                for (int i = 0; i < treeNode.Children.Count; i++)
                {
                    treeNode.Children[i] = getRootFixInfoRecursive(treeNode.Children[i], rootFixes, introducedThrough);
                }
            }

            return treeNode;
        }

        private async Task<List<T2>> getEntitiesAsync<T, T2>(int repositoryId, int commitId, string endpoint, bool failOnError) where T : IDebrickedApiReply<T2>, new() where T2 : new()
        {
            StringBuilder requestUrl = new StringBuilder(endpoint).Append("?repositoryId={0}&commitId={1}");
            List<T> replies = await MakePagedGetRequestAsync<T, T2>(String.Format(requestUrl.ToString(), repositoryId, commitId), failOnError);
            List<T2> result = new List<T2>();
            foreach (var reply in replies)
            {
                result.AddRange(reply.Entities);
            }
            return result;
        }

        public async Task AuthenticateAsync()
        {
            if (checkCredentialsEmpty())
            {
                CredentialPromptWindow prompt = new CredentialPromptWindow(settings);
                prompt.ShowDialog();
                if (!prompt.Authenticated)
                {
                    throw new Exception("Failed to authenticate, missing credentials");
                }
            }
            if (String.IsNullOrEmpty(settings.getDecryptedDebrickedToken()))
            {
                token = await authenticateWithUsernamePasswordAsync(settings.Username, settings.getDecryptedDebrickedPassword());
            }
            else
            {
                token = await authenticateWithTokenAsync(settings.getDecryptedDebrickedToken());
            }

            if (token is null || String.IsNullOrEmpty(token.Token))
            {
                throw new Exception("Authentication failed, token is null");
            }

            if (client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
            client.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", token.Token));
            authenticated = true;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                var tokenReply = await authenticateWithUsernamePasswordAsync(username, password);
                return tokenReply != null && !string.IsNullOrEmpty(tokenReply.Token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateAsync(string token)
        {
            try
            {
                var tokenReply = await authenticateWithTokenAsync(token);
                return tokenReply != null && !string.IsNullOrEmpty(tokenReply.Token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RepositoryExistsAsync(int repositoryId)
        {
            try
            {
                _=await HttpHelper.MakeGetRequestAsync<dynamic>(client, string.Format(ApiEndpoints.VerifyRepoExistsUrl, repositoryId), (e) => { throw e; });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> CreateRepositoryAsync(string repositoryName)
        {
            try
            {
                DebrickedUploadFileReply createRepoReply =await createRepositoryWithEmptyLockFileAsync(repositoryName);
                var commitReply = await finishUploadAsync(createRepoReply.CiUploadId);
                return commitReply.RepositoryId;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create repository " + repositoryName, ex);
            }
        }

        public async Task DeleteRepositoryAsync(int repositoryId)
        {
            await HttpHelper.MakeDeleteRequestAsync(client, string.Format(ApiEndpoints.DeleteRepositoryUrl, repositoryId));
        }

        public async Task<List<DebrickedRepositoryIdentifier>> GetRepositoriesAsync()
        {
            return await HttpHelper.MakeGetRequestAsync<List<DebrickedRepositoryIdentifier>>(client, ApiEndpoints.ListRepositoriesUrl, (e) => throw (e));
        }

        private async Task<DebrickedCommitIdentifier> finishUploadAsync(int ciUploadId)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent(ciUploadId.ToString()), "ciUploadId");
            form.Add(new StringContent("true"), "returnCommitData");
            return await HttpHelper.MakePostRequestAsync<DebrickedCommitIdentifier>(client, ApiEndpoints.FinishUploadUrl, form);
        }

        private async Task<DebrickedUploadFileReply> createRepositoryWithEmptyLockFileAsync(string repositoryName)
        {
            //empty did not work, need a dependency with vuln to get detaillink
            string lockFileLiteral = @"{
  ""version"": 1,
  ""dependencies"": {
    "".NETFramework,Version=v4.7.2"": {
      ""MessagePack"": {
        ""type"": ""Transitive"",
        ""resolved"": ""2.2.85"",
        ""contentHash"": ""3SqAgwNV5LOf+ZapHmjQMUc7WDy/1ur9CfFNjgnfMZKCB5CxkVVbyHa06fObjGTEHZI7mcDathYjkI+ncr92ZQ=="",
        ""dependencies"": {
          ""MessagePack.Annotations"": ""2.2.85"",
          ""Microsoft.Bcl.AsyncInterfaces"": ""1.0.0"",
          ""System.Collections.Immutable"": ""1.5.0"",
          ""System.Memory"": ""4.5.3"",
          ""System.Reflection.Emit"": ""4.6.0"",
          ""System.Reflection.Emit.Lightweight"": ""4.6.0"",
          ""System.Runtime.CompilerServices.Unsafe"": ""4.5.2"",
          ""System.Threading.Tasks.Extensions"": ""4.5.3""
        }
      }
    }
  }
}";
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent(repositoryName), "repositoryName");
            form.Add(new StringContent("initialcommit"), "commitName");
            byte[] jsonContent = Encoding.UTF8.GetBytes("{}");
            form.Add(new ByteArrayContent(jsonContent, 0, jsonContent.Length), "fileData", "packages.lock.json");
            return await HttpHelper.MakePostRequestAsync<DebrickedUploadFileReply>(client, ApiEndpoints.UploadFileUrl, form);
        }

        private bool checkCredentialsEmpty()
        {
            return (string.IsNullOrEmpty(settings.Username) && string.IsNullOrEmpty(settings.Password)) && string.IsNullOrEmpty(settings.DebrickedToken);
        }

        private async Task<DebrickedTokenReply> authenticateWithTokenAsync(string token)
        {
            StringContent content = new StringContent(String.Format("{{\"refresh_token\":\"{0}\"}}", token), UTF8Encoding.UTF8, "application/json");
            return await HttpHelper.MakePostRequestAsync<DebrickedTokenReply>(client, ApiEndpoints.LoginRefresh, content);
        }

        private async Task<DebrickedTokenReply> authenticateWithUsernamePasswordAsync(string username, string password)
        {
            //returns 401 if usernam + pw empty
            StringContent content = new StringContent(String.Format("{{\"_username\":\"{0}\", \"_password\":\"{1}\"}}", username, password), UTF8Encoding.UTF8, "application/json");
            return await HttpHelper.MakePostRequestAsync<DebrickedTokenReply>(client, ApiEndpoints.LoginCheck, content);
        }

        private async Task<List<T>> MakePagedGetRequestAsync<T, T2>(string endpoint, bool failOnError) where T : IDebrickedApiReply<T2>, new() where T2: new()
        {
            int page = 1;
            int pageSize = 100;
            var result = new List<T>();
            StringBuilder requestSb = new StringBuilder(endpoint);
            if (!endpoint.Contains("?"))
            {
                requestSb.Append("?page={0}");
            }
            else
            {
                requestSb.Append("&page={0}");
            }
            requestSb.Append("&rowsPerPage={1}");
            var reply = await HttpHelper.MakeGetRequestAsync<T>(client, String.Format(requestSb.ToString(), page, pageSize), (e) => HandleWebException(e, failOnError));
            while (reply.Entities.Count > 0)
            {
                result.Add(reply);
                page++;
                reply = await HttpHelper.MakeGetRequestAsync<T>(client, String.Format(requestSb.ToString(), page, pageSize), (e) => HandleWebException(e, failOnError));
            }
            return result;
        }

        private void HandleWebException(WebException webException, bool fail)
        {
            if (fail)
            {
                throw webException;
            } else
            {
                Console.WriteLine(webException.ToString());
            }
        }

        internal async Task CopyRepositoryRulesAsync(int tempRepoId, int mappedRepoId)
        {
            var rules = await HttpHelper.MakePostRequestAsync<DebrickedRulesReply>(client, string.Format(ApiEndpoints.RulesUrl, mappedRepoId), new StringContent(""));
            foreach (var rule in rules.RuleGroups)
            {
                if(rule.DefaultRuleIds!=null && rule.DefaultRuleIds.Count > 0) { continue; }
                //get conditions since the next endpoint requires them to be present :( doesnt even work properly, no conditions returned
                dynamic dyn = new ExpandoObject();
                dyn.repositoryIds = rule.RepositoryIds;
                dyn.ruleIds = rule.RuleIds;
                dyn.locale = "en";
                var ruleConditions = await HttpHelper.MakePostRequestAsync<DebrickedRulesDataReply>(client, ApiEndpoints.RulesDataUrl, new StringContent(JsonSerializer.Serialize(dyn)));

                rule.RepositoryIds.Add(tempRepoId);
                string body = JsonSerializer.Serialize(rule);
                var body2 = rule.GetPutContent(ruleConditions.Conditions);
                await HttpHelper.MakePutRequestAsync(client, ApiEndpoints.UpdateRulesUrl, body2);
            }
        }
    }
}
