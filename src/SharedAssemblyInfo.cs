using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("AvalonBar Project")]
[assembly: AssemblyProduct("AvalonBar")]
[assembly: AssemblyCopyright("© AvalonBar Project 2019; © LongBar Project Group 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

internal static class VersionInfo
{
    // Increment for every new milestone/release
    internal const string Core = "1.0.0.0";
    // Increment if Tile API changes
    internal const string Tiles = "1.1.74.0";
    internal const string Configuration
#if DEBUG
        = "Debug";
#else
        = "Release";
#endif
}
