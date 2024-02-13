using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedUploadFileReply
    {
        [JsonPropertyName("ciUploadId")]
        public int CiUploadId { get; set; }

        [JsonPropertyName("uploadProgramsFileId")]
        public int UploadProgramsFileId { get; set; }

        [JsonPropertyName("totalScans")]
        public int TotalScans { get; set; }

        [JsonPropertyName("remainingScans")]
        public int RemainingScans { get; set; }

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; }

        [JsonPropertyName("estimatedDaysLeft")]
        public int EstimatedDaysLeft { get; set; }
    }
}
