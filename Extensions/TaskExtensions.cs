using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Debricked.Extensions
{
    internal static class TaskExtensions
    {
        public static async Task ParallelForeachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int maxDoP)
        {
            SemaphoreSlim limiter = new SemaphoreSlim(maxDoP);
            var tasks = source.Select(async item =>
            {
                await limiter.WaitAsync();
                try
                {
                    await action(item).ConfigureAwait(false);
                }
                finally { limiter.Release(); }
            });
            await Task.WhenAll(tasks);
        }

    }
}
