using Debricked.Models;
using Debricked.Models.Constants;
using Debricked.Models.DebrickedApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Debricked.Helpers
{
    internal static class ConversionHelper
    {
        public static DateTime TimestampMsToDateTime(long timestamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddMilliseconds(timestamp);
            return dt;
        }

        public static T StringToEnum<T>(string str) where T : struct, IConvertible
        {
            if(!typeof(T).IsEnum)
            {
                throw new Exception("Type T must be an Enum");
            }
            return Enum.TryParse(str, true, out T val) ? val : default;
        }

        public static T JsonStringToClass<T>(string str) where T : new()
        {
            if(typeof(T) == typeof(string) && string.IsNullOrEmpty(str)) return new T();
            string t = typeof(T).Name;
            var x = JsonSerializer.Deserialize<T>(str);
            return x;
        }

        public static Vulnerability DebrickedVulnToVuln(DebrickedVulnerability debrickedVuln, Regex re)
        {
            if(re is null)
            {
                re = new Regex(RegEx.FirstIdInLinkRegex);
            }
            return new Vulnerability()
            {
                Id = debrickedVuln.GetId(re),
                CveId = debrickedVuln.CveId,
                Cvss = debrickedVuln.Cvss,
                Discovered = debrickedVuln.Discovered,
                IsDisputed = debrickedVuln.IsDisputed,
                Link = debrickedVuln.Link,
                UsesVulnerableFunctionality = debrickedVuln.UsesVulnerableFunctionality,
                VulnerabilityStatus = debrickedVuln.VulnerabilityStatus,
                DependenciesIds = debrickedVuln.GetDepenciesIds(re),
                DependenciesNames = debrickedVuln.GetDependenciesNames(),
                VulnerabilityTimelineIntervals = debrickedVuln.VulnerabilityTimelineIntervals,
                RefSummaries = debrickedVuln.RefSummaries,
                IsRootFixAvailable = debrickedVuln.IsRootFixAvailable,
                IsDirectFixAvailable = debrickedVuln.IsDirectFixAvailable,
                Trees = debrickedVuln.Trees,
            };
        }

        public static Dictionary<int, Vulnerability> DebrickedVulnListToVulnDict(List<DebrickedVulnerability> debrickedVulns)
        {
            Regex re = new Regex(RegEx.FirstIdInLinkRegex, RegexOptions.Compiled);
            Dictionary<int, Vulnerability> result = new Dictionary<int, Vulnerability>();
            foreach(DebrickedVulnerability vuln in debrickedVulns)
            {
                var newVuln = DebrickedVulnToVuln(vuln, re);
                result.Add(newVuln.Id, newVuln);
            }
            return result;
        }

        public static Dictionary<string, License> DebrickedLicListToLicDict(List<DebrickedLicense> debrickedLics)
        {
            Dictionary<string, License> result = new Dictionary<string, License>();
            foreach (DebrickedLicense lic in debrickedLics)
            {
                result.Add(lic.Name, DebrickedLicToLic(lic));
            }
            return result;
        }

        public static License DebrickedLicToLic(DebrickedLicense license)
        {
            return new License()
            {
                LicenseFamily = license.LicenseFamily,
                Link = license.Link,
                Name = license.Name,
                Risk = license.Risk,
            };
        }

        public static Dictionary<int, Dependency> DebrickedDepListToDepDict(List<DebrickedDependency> deps, Dictionary<int, Rule> rules)
        {
            Dictionary<int, Dependency> result = new Dictionary<int, Dependency>();
            foreach (DebrickedDependency dep in deps)
            {
                result.Add(dep.Id, DebrickedDepToDep(dep, rules));
            }
            return result;
        }

        public static Dependency DebrickedDepToDep(DebrickedDependency dep, Dictionary<int, Rule> rules)
        {
            //get this before so that the rule-indipendent conditions are filled
            var conditions = dep.GetTriggeredConditions(rules);
            return new Dependency()
            {
                ContributorsScore = dep.ContributorsScore,
                Id = dep.Id,
                SelectId = dep.SelectId,
                IndirectDependencies = DebrickedDepListToDepDict(dep.IndirectDependencies, rules),
                IsMatched = dep.IsMatched,
                IsRoot = dep.IsRoot,
                LicenseNames = dep.Licenses.Select(dict => dict["name"]).ToHashSet(),
                Link = dep.Link,
                Name = dep.Name,
                PopularityScore = dep.PopularityScore,
                SecurityScore = dep.SecurityScore,
                ShortName = dep.ShortName,
                TotalDirectVulnerabilities = dep.TotalDirectVulnerabilities,
                TotalVulnerabilities = dep.TotalVulnerabilities,
                PolicyStatus = dep.PolicyStatus,
                TriggeredConditions = conditions,
                RuleIndependentTriggeredConditions = dep.RuleIndependentTriggeredConditions,
                IntroducedThroughFiles = dep.IntroducedThroughFiles,
            };
        }

        public static Rule DebrickedRuleGroupToRule(DebrickedRuleGroup debrickedRuleGroup, int repositoryId)
        {
            debrickedRuleGroup.Description = HttpUtility.HtmlDecode(debrickedRuleGroup.Description);
            debrickedRuleGroup.Description = debrickedRuleGroup.Description.Replace("</p>", "\n");
            //there is a leading space on the last line that is not present on the description from the api result reply, to guarantee proper matching it needs to be removed
            debrickedRuleGroup.Description = debrickedRuleGroup.Description.Replace("\n<p> ", "\n");
            debrickedRuleGroup.Description = Regex.Replace(debrickedRuleGroup.Description, "<.*?>", String.Empty);
            if(debrickedRuleGroup.Description.EndsWith("\n")) { debrickedRuleGroup.Description = debrickedRuleGroup.Description.Substring(0, debrickedRuleGroup.Description.Length-1); }
            return new Rule()
            {
                Id = debrickedRuleGroup.GetRuleId(repositoryId),
                Actions = debrickedRuleGroup.Actions,
                Active = debrickedRuleGroup.Active,
                Description = debrickedRuleGroup.Description,
            };
        }

        public static Dictionary<int, Rule> DebrickedRuleGroupListToRuleDict(List<DebrickedRuleGroup> debrickedRuleGroupList, int repositoryId)
        {
            Dictionary<int, Rule> result = new Dictionary<int, Rule>();
            foreach (DebrickedRuleGroup item in debrickedRuleGroupList)
            {
                Rule r = DebrickedRuleGroupToRule(item, repositoryId);
                result.Add(r.Id, r);
            }
            return result;
        }

        public static Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> DebrickedUploadResultReplyToDict(DebrickedUploadResultReply reply)
        {
            //dependencyId, ruleDescription, subject, values
            Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>> result = new Dictionary<int, Dictionary<int, Dictionary<string, HashSet<string>>>>();
            //TODO switch regex?
            Regex re = new Regex(RegEx.FirstIdInLinkRegex, RegexOptions.Compiled);
            Regex re2 = new Regex(RegEx.ParameterIdAtEndRegex, RegexOptions.Compiled);
            foreach (var automationRule in reply.AutomationRules)
            {
                if (!automationRule.IsTriggered) continue;
                int ruleId = automationRule.GetId(re2);
                foreach (var triggerEvent in automationRule.TriggerEvents)
                {
                    int dependencyId = triggerEvent.GetDependencyId(re);
                    if (!result.ContainsKey(dependencyId))
                    {
                        result.Add(dependencyId, new Dictionary<int, Dictionary<string, HashSet<string>>>());
                    }

                    if (!result[dependencyId].ContainsKey(ruleId))
                    {
                        result[dependencyId].Add(ruleId, new Dictionary<string, HashSet<string>>());
                    }

                    if (automationRule.HasCves)
                    {
                        if (!result[dependencyId][ruleId].ContainsKey(RuleSubjectLabels.Cve)) { result[dependencyId][ruleId].Add(RuleSubjectLabels.Cve, new HashSet<string>()); }
                        result[dependencyId][ruleId][RuleSubjectLabels.Cve].Add(triggerEvent.Cve);
                    }

                    if (automationRule.Description.Contains(RuleSubjectLabels.License))
                    {
                        if (!result[dependencyId][ruleId].ContainsKey(RuleSubjectLabels.License)) { result[dependencyId][ruleId].Add(RuleSubjectLabels.License, new HashSet<string>()); }
                        triggerEvent.Licenses.ForEach(x => result[dependencyId][ruleId][RuleSubjectLabels.License].Add(x));
                    }
                }
            }
            return result; 
        }
    }
}
