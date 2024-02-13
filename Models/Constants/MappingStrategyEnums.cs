namespace Debricked.Models.Constants
{
    public enum MappingStrategyEnum
    {
        Persistent,
        Temporary
    }

    public enum PersistentRepoStrategy
    {
        AutoDetectThenSolutionName,
        AutoDetectThenAskRepoID,
        AlwaysAskRepoID
    }

    public enum TempRepoStrategy
    {
        AlwaysAskRepoID,
        NoMapping
    }
}
