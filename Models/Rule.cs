using Debricked.Models.Constants;
using System.Collections.Generic;

namespace Debricked.Models
{
    internal class Rule
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }

        public HashSet<string> Actions { get; set; }


        public PolicyStatusEnum GetPolicyStatus()
        {
            if (this.Actions.Contains("failPipeline")) { return PolicyStatusEnum.Fail; }
            else if (this.Actions.Contains("warnPipeline")) { return PolicyStatusEnum.Warn; }
            else { return PolicyStatusEnum.Pass; }
        }
    }
}
