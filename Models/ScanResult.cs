using Community.VisualStudio.Toolkit;
using Debricked.Models.Constants;
using Debricked.Models.DebrickedApi;
using Microsoft.VisualStudio.PlatformUI.OleComponentSupport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Models
{
    internal class ScanResult
    {
        //intended to hold either the same value as RepositoryId or the repositoryId from which to pull rules if RepositoryId is a temporary repository
        public DebrickedRepositoryIdentifier MappedRepository { get; set; } =  null;

        public MappingStrategyEnum MappingStrategy { get; set; }

        public int RepositoryId { get; set; }

        public int CommitId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; } = null;

        [JsonIgnore]
        public Solution Solution { get; set; } = null;

        [JsonIgnore]
        public String Scope { get { return getScope(); } set { this.Scope = value; } }

        public DateTime LastFullRefresh { get; set; } = DateTime.MinValue;

        public DateTime LastRefresh { get; set; } = DateTime.MinValue;

        //id / dependency
        public ConcurrentDictionary<int, Dependency> Dependencies { get; set; } = new ConcurrentDictionary<int, Dependency>();

        //id / vuln
        public ConcurrentDictionary<int, Vulnerability> Vulnerabilities { get; set; } = new ConcurrentDictionary<int, Vulnerability>();

        //id / parentId (null if root dependency)
        public ConcurrentDictionary<int, int?> DependencyIds { get; set; } = new ConcurrentDictionary<int, int?>();

        //vulnId / dependencyId
        public ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> VulnToDepMapping { get; set; } = new ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> { };

        public Dictionary<int, Rule> Rules { get; set; } = new Dictionary<int, Rule>();

        public ScanResult(int repositoryId, int commitId, Dictionary<int, Vulnerability> vulnerabilities, Dictionary<int, Dependency> dependencies, Dictionary<string, License> licenses, Dictionary<int, Rule> rules, Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping)
        {
            this.RepositoryId = repositoryId;
            this.CommitId = commitId;
            this.Dependencies = new ConcurrentDictionary<int, Dependency>(dependencies);
            this.Vulnerabilities = new ConcurrentDictionary<int, Vulnerability>(vulnerabilities);
            this.Rules = rules;
            this.LastRefresh = DateTime.Now;
            this.LastFullRefresh = DateTime.Now;

            Parallel.ForEach(this.Dependencies, dependency =>
            {
                this.Dependencies[dependency.Key] = mapAndEnrichDependency(licenses, depPolicyMapping, dependency);
            });

            Parallel.ForEach(vulnerabilities, vuln =>
            {
                mapVuln(vuln);
            });

        }

        private Dependency mapAndEnrichDependency(Dictionary<string, License> licenses, Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping, KeyValuePair<int, Dependency> dependency)
        {
            this.DependencyIds.TryAdd(dependency.Key, null);
            foreach (string licenseName in dependency.Value.LicenseNames)
            {
                if (licenses.ContainsKey(licenseName))
                {
                    dependency.Value.Licenses.Add(licenseName, licenses[licenseName]);
                }
            }

            foreach (var childDependency in dependency.Value.IndirectDependencies)
            {
                childDependency.Value.SecurityScore = childDependency.Value.SecurityScore == -1 ? -1 : childDependency.Value.SecurityScore / 10;
                childDependency.Value.ContributorsScore = childDependency.Value.ContributorsScore == -1 ? -1 : childDependency.Value.ContributorsScore / 10;
                childDependency.Value.PopularityScore = childDependency.Value.PopularityScore == -1 ? -1 : childDependency.Value.PopularityScore / 10;
                this.DependencyIds.TryAdd(childDependency.Key, dependency.Key);
                foreach (string licenseName in childDependency.Value.LicenseNames)
                {
                    if (licenses.ContainsKey(licenseName))
                    {
                        childDependency.Value.Licenses.Add(licenseName, licenses[licenseName]);
                    }
                }
                //check for child dependencies that have triggered rules and add data to triggeredconditions
                if (depPolicyMapping.ContainsKey(childDependency.Key))
                {
                    dependency = mapChildPolicyData(depPolicyMapping, dependency, childDependency);
                }
            }
            return dependency.Value;
        }

        private KeyValuePair<int, Dependency> mapChildPolicyData(Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping, KeyValuePair<int, Dependency> dependency, KeyValuePair<int, Dependency> childDependency)
        {
            foreach (var ruleDescriptionNode in depPolicyMapping[childDependency.Key])
            {
                var rule = Rules[ruleDescriptionNode.Key];
                if (!dependency.Value.TriggeredConditions.ContainsKey(rule.Description))
                {
                    dependency.Value.TriggeredConditions.Add(rule.Description, new Dictionary<string, HashSet<string>>());
                }

                if (!dependency.Value.TriggeredConditions[rule.Description].ContainsKey(RuleSubjectLabels.IndirectDependency))
                {
                    dependency.Value.TriggeredConditions[rule.Description].Add(RuleSubjectLabels.IndirectDependency, new HashSet<string>());
                }
                string value = childDependency.Value.Name;
                if (ruleDescriptionNode.Value.ContainsKey(RuleSubjectLabels.Cve))
                {
                    value = new StringBuilder(value).Append(" | (").Append(string.Join(",", ruleDescriptionNode.Value[RuleSubjectLabels.Cve])).Append(")").ToString();
                }

                if (rule.Description.Contains(RuleSubjectLabels.License))
                {
                    value = new StringBuilder(value).Append(" | (license: ").Append(string.Join(",", ruleDescriptionNode.Value[RuleSubjectLabels.License])).Append(')').ToString();
                }
                dependency.Value.TriggeredConditions[rule.Description][RuleSubjectLabels.IndirectDependency].Add(value);

                //if the error/warn level of the triggered rule is higher than the level of the root dependency then bubble up
                if (rule.GetPolicyStatus() > dependency.Value.PolicyStatus) { dependency.Value.PolicyStatus = rule.GetPolicyStatus(); }
            }

            return dependency;
        }

        public ScanResult()
        {
            
        }

        public List<Vulnerability> GetVulnerabilities(int vulnerabilityId)
        {
            List<Vulnerability> result = new List<Vulnerability>();
            if (!this.VulnToDepMapping.ContainsKey(vulnerabilityId))
            {
                return result;
            }

            foreach (int dependencyId in this.VulnToDepMapping[vulnerabilityId].Keys)
            {
                if (this.DependencyIds.ContainsKey(dependencyId) && !this.DependencyIds[dependencyId].HasValue)
                {
                    result.Add(this.Dependencies[dependencyId].Vulnerabilities[vulnerabilityId]);
                } else
                {
                    result.Add(this.Dependencies[this.DependencyIds[dependencyId].Value].IndirectDependencies[dependencyId].Vulnerabilities[vulnerabilityId]);
                }
            }
            return result;

        }

        private String getScope()
        {
            if(Solution is null && !(Project is null))
            {
                if(Project.Parent is null)
                {
                    return Project.Name;
                } else
                {
                    return Project.Parent.Name + "/" + Project.Name;
                }
            } else if(!(Solution is null))
            {
                return Solution.Name;
            } else if (!String.IsNullOrEmpty(this.Scope))
            {
                return this.Scope;
            }
            return "null";
        }

        public void MergeNewVulnerabilitiesAndDependencies(Dictionary<int, Vulnerability> vulnerabilities, Dictionary<int, Dependency> dependencies, Dictionary<string, License> licenses, int commitId, Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> depPolicyMapping)
        {
            Parallel.ForEach(dependencies, dependency =>
            {
                if (this.DependencyIds.ContainsKey(dependency.Key))
                {
                    this.Dependencies[dependency.Key].IntroducedThroughFiles = dependency.Value.IntroducedThroughFiles;
                    this.Dependencies[dependency.Key].PolicyStatus = dependency.Value.PolicyStatus;

                    foreach (var childDependency in this.Dependencies[dependency.Key].IndirectDependencies)
                    {
                        if (depPolicyMapping.ContainsKey(childDependency.Key))
                        {
                            this.Dependencies[dependency.Key] = mapChildPolicyData(depPolicyMapping, new KeyValuePair<int, Dependency>(dependency.Key,  this.Dependencies[dependency.Key] ), childDependency).Value;
                        }
                    }

                    return;
                }
                else
                {
                    var dep = mapAndEnrichDependency(licenses, depPolicyMapping, dependency);
                    this.Dependencies.TryAdd(dependency.Key, dep);
                }
            });

            Parallel.ForEach(vulnerabilities, vuln =>
            {
                if (this.Vulnerabilities.ContainsKey(vuln.Key)) return;
                mapVuln(vuln);
                this.Vulnerabilities.TryAdd(vuln.Key, vuln.Value);
            });
            this.LastRefresh = DateTime.Now;
        }

        private void mapVuln(KeyValuePair<int, Vulnerability> vuln)
        {
            this.VulnToDepMapping.TryAdd(vuln.Key, new ConcurrentDictionary<int, byte>());
            foreach (int dependencyId in vuln.Value.DependenciesIds)
            {
                this.VulnToDepMapping[vuln.Key].TryAdd(dependencyId, 0);
                if (this.Dependencies.ContainsKey(dependencyId) && this.DependencyIds[dependencyId].HasValue)
                {
                    this.Dependencies[this.DependencyIds[dependencyId].Value].IndirectDependencies[dependencyId].Vulnerabilities.TryAdd(vuln.Key, vuln.Value);
                }
                else if (this.Dependencies.ContainsKey(dependencyId))
                {
                    this.Dependencies[dependencyId].Vulnerabilities.TryAdd(vuln.Key, vuln.Value);
                }
            }
        }
    }
}
