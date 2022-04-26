namespace RunScript;

using System.Runtime.InteropServices;

public class WindowsTheoryAttribute : TheoryAttribute
{
    public WindowsTheoryAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Ignored on Unix";
        }
    }
}
