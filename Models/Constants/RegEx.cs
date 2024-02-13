namespace Debricked.Models.Constants
{
    internal class RegEx
    {
        public const string DetailsLinkRegex = @"https:\/\/debricked\.com\/app\/en\/repository\/([0-9]*)\/commit\/([0-9]*)";
        public const string FirstIdInLinkRegex = @"^[\/a-zA-Z:\.]{0,}([0-9]*)";
        public const string ParameterIdAtEndRegex = @"^[\S]{0,}=([0-9]*)";
    }
}
