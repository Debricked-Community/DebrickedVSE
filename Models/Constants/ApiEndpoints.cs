namespace Debricked.Models.Constants
{
    internal class ApiEndpoints
    {

        //Github
        public const string DebrickedCliReleaseEndpoint = "https://api.github.com/repos/debricked/cli/releases/latest";

        //Debricked
        public const string DebrickedBaseUrl = "https://debricked.com";
        public const string DebrickedApiVersion = "1.0";

        public const string LoginRefresh = DebrickedBaseUrl + "/api/login_refresh"; //login with token
        public const string LoginCheck = DebrickedBaseUrl + "/api/login_check"; //login with username/password
        public const string VulnerabilitiesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerabilities/get-vulnerabilities";
        public const string DependenciesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/dependencies/get-dependencies-hierarchy";
        public const string LicensesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/licenses/get-licenses";
        public const string RootFixUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerability/{0}/repositories/{1}/root-fixes?commitId={2}";
        public const string VulnerabilityFilesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerability/{0}/files?commitId={1}";
        public const string FixDependencyTreeUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerability/{0}/files/{1}/dependency-tree?commitId={2}";
        public const string VulnTimeLineUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerability/{0}/vulnerable-timeline?repositoryId={1}";
        public const string RefSummaryUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/vulnerability/{0}/refsummary";
        public const string RulesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/automations/{0}/get-rules";
        public const string RulesDataUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/automations/rules/data";
        public const string UpdateRulesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/automations/save-rules";
        public const string DeleteRepositoryUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/repository-settings/repositories/remove/{0}";
        public const string DependencyFilesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/dependency/{0}/vulnerable-files?repositoryId={1}&commitId={2}";
        public const string DependencyInfoUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/dependency/{0}";
        public const string VerifyRepoExistsUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/scan/latest-scan-status?repositoryId={0}";
        public const string ListRepositoriesUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/repositories/get-repositories-names-links";
        public const string UploadFileUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/uploads/dependencies/files";
        public const string FinishUploadUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/finishes/dependencies/files/uploads";
        public const string UploadStatusUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/ci/upload/status?ciUploadId={0}";
        public const string SelectDataUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/open/select-data/dependency-data/{0}";
        public const string GetSelectIdUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/select/get-dependency-id/{0}";
        public const string OshDataUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/select/get-osh-data/{0}";
        public const string PolicyHintsUrl = DebrickedBaseUrl + "/api/" + DebrickedApiVersion + "/select/get-policy-hints-data/{0}?repositoryIds={1}&policyIds=";
    }
}
