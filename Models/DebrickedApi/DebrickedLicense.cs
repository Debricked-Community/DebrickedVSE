using Debricked.Models.Constants;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedLicense
    {
        #region Public properties
        [JsonIgnore]
        public string Name { get { return this.LicenseName.Name; } }

        [JsonIgnore]
        public string Link { get { return this.LicenseName.Link; } }

        [JsonIgnore]
        public LicenseRiskEnum Risk { get { return LicenseRisks.Find((l) => l.Amount > 0).Type; } }

        [JsonPropertyName("licenseFamily")]
        public string LicenseFamily { get; set; }

        [JsonPropertyName("licenseRisks")]
        public List<DebrickedLicenseRisk> LicenseRisks { get; set; } = new List<DebrickedLicenseRisk>();

        [JsonPropertyName("name")]
        public DebrickedLicenseName LicenseName { get; set; }
        #endregion

        #region Classes
        public class DebrickedLicenseName
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("link")]
            public string Link { get; set; }
        }

        public class DebrickedLicenseRisk
        {
            [JsonPropertyName("type")]
            public LicenseRiskEnum Type { get; set; }

            [JsonPropertyName("amount")]
            public int Amount { get; set; }
        }
        #endregion
    }

}
