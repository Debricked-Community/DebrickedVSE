using Debricked.Models.DebrickedApi;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal static class ProcessHelper
    {
        public static async Task<RunProcessResult> RunProcessAsync(string fileName, string args)
        {
            using (var process = new Process
            {
                StartInfo =
                    {
                        FileName = fileName, Arguments = args,
                        UseShellExecute = false, CreateNoWindow = true,
                        RedirectStandardOutput = true, RedirectStandardError = true
                    },
                EnableRaisingEvents = true
            })
            {
                return await RunProcessAsync(process).ConfigureAwait(false);
            }
        }
        private static Task<RunProcessResult> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<RunProcessResult>();

            StringBuilder stdErr = new StringBuilder();
            StringBuilder stdOut = new StringBuilder();

            process.Exited += (s, ea) => { 
                tcs.SetResult(new RunProcessResult
                {
                    ExitCode = process.ExitCode,
                    StdErr = stdErr.ToString(),
                    StdOut = stdOut.ToString()
                }); };
            process.OutputDataReceived += (s, ea) => stdOut.Append(ea.Data);
            process.ErrorDataReceived += (s, ea) => stdErr.Append(ea.Data);

            bool started = process.Start();
            if (!started)
            {
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }

    public class RunProcessResult
    {
        public int ExitCode;
        public string StdErr;
        public string StdOut;
        public DebrickedRepositoryIdentifier MappedRepository = null;
    }
}
