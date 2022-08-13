#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.

namespace RunScript;

using System.Diagnostics;

internal static class TaskExtensions
{
#if !NET5_0_OR_GREATER
    public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        process.WaitForExit();
    }
#endif

#if !NET6_0_OR_GREATER
    public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        task.Wait(cancellationToken);
    }
#endif
}
