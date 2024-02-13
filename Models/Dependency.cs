using Debricked.Models.Constants;
using Debricked.Models.DebrickedApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace Debricked.Models
{
    internal class Dependency
    {
        public int Id { get; set; }

        public int SelectId { get; set; } = -1;

        public string Name { get; set; }

        public string ShortName { get; set; }

        public PolicyStatusEnum PolicyStatus { get; set; } = PolicyStatusEnum.Fail;

        [JsonIgnore]
        public Brush PolicyStatusBrush { get { return getPolicyStatusBrush(); } }

        [JsonIgnore]
        public string PolicyStatusString { get { return Enum.GetName(typeof(PolicyStatusEnum), PolicyStatus); } }

        public string Link { get; set; }

        public bool IsRoot { get; set; }

        public bool IsMatched { get; set; }

        public int TotalDirectVulnerabilities { get; set; }

        public int TotalVulnerabilities { get; set; }

        public int ContributorsScore { get; set; } = -1;

        [JsonIgnore]
        public Brush ContributorsScoreBrush { get {  return getScoreBrush(ContributorsScore); } }

        public int PopularityScore { get; set; } = -1;

        [JsonIgnore]
        public Brush PopularityScoreBrush { get { return getScoreBrush(PopularityScore); } }

        public int SecurityScore { get; set; } = -1;

        [JsonIgnore]
        public Brush SecurityScoreBrush { get { return getScoreBrush(SecurityScore); } }

        public HashSet<string> LicenseNames { get; set; }

        public List<string> IntroducedThroughFiles { get; set; } = new List<string>();

        public Dictionary<int, Dependency> IndirectDependencies { get; set; } = new Dictionary<int, Dependency>();

        public ConcurrentDictionary<int, Vulnerability> Vulnerabilities { get; set; } = new ConcurrentDictionary<int, Vulnerability>();

        public Dictionary<string, License> Licenses { get; set; } = new Dictionary<string, License>();

        [JsonIgnore]
        public IEnumerable<Dependency> AsCollection { get {return new List<Dependency>() { this }; } }

        //rule description, subject, values
        public Dictionary<string, Dictionary<string, HashSet<string>>> TriggeredConditions { get; set; } = new Dictionary<string, Dictionary<string, HashSet<string>>>();

        public Dictionary<string, HashSet<string>> RuleIndependentTriggeredConditions { get; set; } = new Dictionary<string, HashSet<string>>();

        [JsonIgnore]
        public string PolicyStatusTooltip { get { return getPolicyStatusTooltip(); } }

        [JsonIgnore]
        public Visibility VulnerabilitiesVisibility {  get { return this.Vulnerabilities.Count > 0 ? Visibility.Visible : Visibility.Hidden; } }
        [JsonIgnore]
        public GridLength VulnerabilitesHeight { get { return this.Vulnerabilities.Count > 0 ? GridLength.Auto : new GridLength(0); } }
        [JsonIgnore]
        public Visibility IndirectDependenciesVisibility { get { return this.IndirectDependencies.Count > 0 ? Visibility.Visible : Visibility.Hidden; } }
        [JsonIgnore]
        public GridLength IndirectDependenciesHeight { get { return this.IndirectDependencies.Count > 0 ? GridLength.Auto : new GridLength(0); } }
        [JsonIgnore]
        public Visibility TriggeredConditionsVisibility { get { return this.TriggeredConditions.Count > 0 ? Visibility.Visible : Visibility.Hidden; } }
        [JsonIgnore]
        public GridLength TriggeredConditionsHeight { get { return this.TriggeredConditions.Count > 0 ? GridLength.Auto : new GridLength(0); } }
        [JsonIgnore]
        public Visibility LicensesVisibility { get { return this.LicenseNames.Count > 0 ? Visibility.Visible : Visibility.Hidden; } }
        [JsonIgnore]
        public GridLength LicensesHeight { get { return this.LicenseNames.Count > 0 ? GridLength.Auto : new GridLength(0); } }

        private Brush getScoreBrush(int score)
        {
            if (score < 0)
            {
                return Brushes.Transparent;
            } else if(score < 30)
            {
                return Brushes.Red;
            } else if(score < 70)
            {
                return Brushes.Yellow;
            } else
            {
                return Brushes.Green;
            }
        }

        private Brush getPolicyStatusBrush()
        {
            switch(PolicyStatus)
            {
                case PolicyStatusEnum.Pass: return Brushes.Green;
                case PolicyStatusEnum.Fail: return Brushes.Red;
                case PolicyStatusEnum.Warn: return Brushes.Yellow;
                default: return Brushes.Transparent;
            }
        }

        private string getPolicyStatusTooltip()
        {
            if(this.PolicyStatus == PolicyStatusEnum.Pass)
            {
                return "Policy Status: Pass";
            } else
            {
                string prefix = "";
                StringBuilder sb = new StringBuilder("Policy Status: ").Append(Enum.GetName(typeof(PolicyStatusEnum), this.PolicyStatus));
                sb.AppendLine().Append("Triggered Conditions:");
                foreach (var triggeredConditionSubject in RuleIndependentTriggeredConditions)
                {
                    sb.AppendLine().Append(triggeredConditionSubject.Key).Append(": ");
                    foreach (var triggeredConditionValue in triggeredConditionSubject.Value)
                    {
                        sb.Append(prefix).Append(triggeredConditionValue);
                        prefix = ",";
                    }
                    prefix = "";
                }
                return sb.ToString();
            }
        }

        public string GetErrorListDescription()
        {
            if (this.PolicyStatus == PolicyStatusEnum.Pass)
            {
                return "This error should not exist";
            } else
            {
                string prefix = "";
                StringBuilder sb = new StringBuilder("The dependency ").Append(this.Name).Append(" has failed policy checks with the status '").Append(this.PolicyStatusString).Append("'");
                sb.AppendLine().Append("Triggered Conditions:");
                foreach (var triggeredConditionSubject in RuleIndependentTriggeredConditions)
                {
                    sb.AppendLine().Append(triggeredConditionSubject.Key).Append(": ");
                    foreach (var triggeredConditionValue in triggeredConditionSubject.Value)
                    {
                        sb.Append(prefix).Append(triggeredConditionValue);
                        prefix = ",";
                    }
                    prefix = "";
                }
                sb.AppendLine().Append("Introduced through file(s):");
                foreach (var file in this.IntroducedThroughFiles)
                {
                    sb.AppendLine().Append(file);
                }
                return sb.ToString();
            }
        }
    }
}
