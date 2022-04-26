namespace RunScript;

using System.Runtime.InteropServices;

public class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Ignored on Unix";
        }
    }
}
