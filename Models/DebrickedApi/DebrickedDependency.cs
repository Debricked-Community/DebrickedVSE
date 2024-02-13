using Debricked.Converters;
using Debricked.Models.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedDependency
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonIgnore]
        public int SelectId { get; set; } = -1;

        [JsonIgnore]
        public string Name { get { return this.DependencyName.Name; } }

        [JsonIgnore]
        public string ShortName { get { return this.DependencyName.ShortName; } }

        [JsonIgnore]
        public string Link { get { return this.DependencyName.Link; } set { this.DependencyName.Link = value; } }

        [JsonIgnore]
        public PolicyStatusEnum PolicyStatus { get; set; }

        [JsonIgnore]
        public DebrickedPolicyHintsReply PolicyData { get; set; }

        [JsonPropertyName("isRoot")]
        [JsonConverter(typeof(JsonBooleanConverter))]
        public bool IsRoot { get; set; }

        [JsonPropertyName("isMatched")]
        public bool IsMatched { get; set; }

        [JsonPropertyName("totalDirectVulnerabilities")]
        public int TotalDirectVulnerabilities { get; set; }

        [JsonPropertyName("totalVulnerabilities")]
        public int TotalVulnerabilities { get; set; }

        [JsonConverter(typeof(JsonIntegerConverter))]
        [JsonPropertyName("contributors")]
        public int ContributorsScore { get; set; } = -1;

        [JsonConverter(typeof(JsonIntegerConverter))]
        [JsonPropertyName("popularity")]
        public int PopularityScore { get; set; } = -1;

        //currently not returned
        [JsonConverter(typeof(JsonIntegerConverter))]
        [JsonPropertyName("security")]
        public int SecurityScore { get; set; } = -1;

        [JsonPropertyName("licenses")]
        public Dictionary<string, string>[] Licenses { get; set; }

        [JsonPropertyName("vulnerabilityPriority")]
        public List<DebrickedVulnerabilityPriority> VulnerabilityPriority { get; set; } = new List<DebrickedVulnerabilityPriority>();

        [JsonPropertyName("indirectDependencies")]
        public List<DebrickedDependency> IndirectDependencies { get; set; } = new List<DebrickedDependency>();
       
        [JsonPropertyName("name")]
        public DebrickedDependencyName DependencyName { get; set; }

        [JsonIgnore]
        public List<string> IntroducedThroughFiles { get; set; }

        [JsonIgnore]
        public Dictionary<string, HashSet<string>> RuleIndependentTriggeredConditions { get; set; } = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, Dictionary<string, HashSet<string>>> GetTriggeredConditions(Dictionary<int, Rule> rules)
        {
            Dictionary<string, Dictionary<string, HashSet<string>>> result = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            if(PolicyData != null && PolicyData.Triggered != null && PolicyData.Triggered.Count>0)
            {
                foreach (var triggeredRule in PolicyData.Triggered)
                {
                    result.Add(rules[triggeredRule.RuleId].Description, new Dictionary<string, HashSet<string>>());
                    foreach (var triggeredConditionGroup in triggeredRule.TriggeredConditions)
                    {
                        foreach (var triggeredCondition in triggeredConditionGroup)
                        {
                            if (triggeredCondition.Subject.Equals("securityScore"))
                            {
                                triggeredCondition.Values.Clear();
                                triggeredCondition.Values.Add(this.SecurityScore.ToString());
                            }
                            if (!result[rules[triggeredRule.RuleId].Description].ContainsKey(triggeredCondition.Subject))
                            {
                                result[rules[triggeredRule.RuleId].Description].Add(triggeredCondition.Subject, new HashSet<string>());
                            }
                            if (!RuleIndependentTriggeredConditions.ContainsKey(triggeredCondition.Subject))
                            {
                                RuleIndependentTriggeredConditions.Add(triggeredCondition.Subject, new HashSet<string>());
                            }
                            triggeredCondition.Values.ForEach(x => { 
                                result[rules[triggeredRule.RuleId].Description][triggeredCondition.Subject].Add(x); 
                                RuleIndependentTriggeredConditions[triggeredCondition.Subject].Add(x); 
                            });
                        }
                    }
                }
            }
            return result;
        }

        #region Classes
        public class DebrickedVulnerabilityPriority
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("amount")]
            public int Amount { get; set; }
        }

        internal class DebrickedDependencyName
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("shortName")]
            public string ShortName { get; set; }

            [JsonPropertyName("link")]
            public string Link { get; set; }

            public int GetId(Regex re)
            {
                var matches = re.Matches(this.Link);
                if (matches.Count >= 1 && matches[0].Groups.Count == 2)
                {
                    return int.Parse(matches[0].Groups[1].Value);
                } else
                {
                    throw new Exception("Unable to extract dependency id from link " + this.Link);
                }
            }
        }
        #endregion
    }
}
