namespace RunScript;

using System.Runtime.InteropServices;

public class UnixFactAttribute : FactAttribute
{
    public UnixFactAttribute()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Ignored on Windows";
        }
    }
}
