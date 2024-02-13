using System.Collections.Generic;

namespace Debricked.Models.DebrickedApi
{
    internal interface IDebrickedApiReply<T>
    {
        List<T> Entities { get; }
    }
}
