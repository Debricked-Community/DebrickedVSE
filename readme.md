# Debricked Visual Studio Extension

readme in progress, check contributing.md if you want to contribute

## Setup (Visual Studio 2022 required)

- Download the latest release in the releases section
- run the .vsix file to install the extension
- open Visual Studio
- Open the Tools -> Options menu and select "Debricked" in the list on the left hand side

### Authentication options (required)
Either ["Debricked Token"](https://docs.debricked.com/product/administration/generate-access-token) or "Debricked Username" AND "Debricked Password" are required to establish a connection to the Debricked API (Debricked Enterprise is required)

<details>
  <summary>### Other options (recommended)</summary>

### Connection options (optional)
Configure an optional proxy and enable "Ignore certificate errors" if required

### Scan settings (recommended)
- Data refresh interval: Number of hours until a full data refresh is done. Until the timeout is reached the extension incrementally updates collected data to reduce the number of API calls.
- Enable Fingerprinting: If enabled the debricked scan will include a [binary fingerprint analysis](https://docs.debricked.com/tools-and-integrations/cli/debricked-cli/file-fingerprinting)
- Exclusions: Items to exclude during resolve and scan actions (one per line)
- Scan entire Solution: If enabled the entire solution (all projects) is scanned. If disabled only the active project is scanned.
- Repository type: See description below
- Persistent repository mapping strategy: See description below
- Temporary repository mapping strategy: See description below


Repository mapping:
There are two types of repository mapping the extension can use, the chosen repository type and associated mapping strategy have a major impact on data retention and hygiene.
The two types of repositories are "persistent" and "temporary" repositories. 


Persistent Repositories:

When using a persistent repository the extension will map a Visual Studio solution or project to a Debricked repository (either existing or automatically created). The repository in question will persist and each consecutive scan will be uploaded to the mapped repository. Be aware that Debricked essentially overwrites old results with new ones. Dont apply this on repositories you are actually using to monitor your pipeline health.
When using a persistent repository the extension will incrementally update new scan results to preserve API calls.

For persistent repositories there are 3 mapping strategy options:
- AutoDetectThenSolutionName: The extension will run ["debricked scan"](https://docs.debricked.com/tools-and-integrations/cli/debricked-cli#scan) without the -repository and -commit parameter the first time a project is scanned. If the debricked CLI fails to detect a repository the solution- or projectname will be used as the repository name.
- AutoDetectThenRepoID: The extension will run ["debricked scan"](https://docs.debricked.com/tools-and-integrations/cli/debricked-cli#scan) without the -repository and -commit parameter the first time a project is scanned. If the debricked CLI fails to detect a repository the extension will present the user with a list of existing repositories to choose from.
- AlwaysAskRepoID: The extension will always present the user with a list of existing repositories to choose from.

Temporary Repositories:

When using a temporary repository the extension will create a repository for each scan. The repository will be deleted automatically once results have been pulled from the API. 
Mapping strategies for temporary repositories enable you to link these temporary repositories with an existing persistent one. This mapping is used by the extension to copy any applicable rules from the mapped repository to ensure proper policy validation.

For temporary repositories there are 2 mapping strategy options:
- AlwaysAskRepoID: The extension will always present the user with a list of existing repositories to choose from.
- NoMapping: No mapping is applied, only rules marked as default rules will be evaluated.

### Triggers (optional)
- After build (always): Triggers a rescan after the Solution or Project is built, regardless of the build being followed by a debug session
- After build (unless debugging): Triggers a rescan after the Solution or Project is built, unless the build is followed by a debug session
- On reference added: Triggers a rescan when a reference is added (currently supported for C#, VB)
  
</details>
