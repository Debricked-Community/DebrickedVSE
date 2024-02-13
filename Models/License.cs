using Debricked.Models.Constants;
using System;

namespace Debricked.Models
{
    internal class License
    {
        public String Name { get; set; }

        public String Link { get; set; }

        public LicenseRiskEnum Risk { get; set; }

        public String LicenseFamily { get; set; }
    }
}
