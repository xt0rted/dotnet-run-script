namespace RunScript;

using System.Runtime.InteropServices;

public class UnixTheoryAttribute : TheoryAttribute
{
    public UnixTheoryAttribute()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Ignored on Windows";
        }
    }
}
