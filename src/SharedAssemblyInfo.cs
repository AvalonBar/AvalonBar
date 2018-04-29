using System.Reflection;
using System.Runtime.InteropServices;

// The SharedAssemblyInfo file contains parameters shared by the
// projects included in this solution. It helps in changing versions
// and branch information easily without going into every AssemblyInfo
// file with every version/milestone change.

[assembly: AssemblyCompany("The AvalonBar Project")]
[assembly: AssemblyProduct(GitInfo.BranchProdName)]
[assembly: AssemblyCopyright("Portions © The AvalonBar Project 2018. Portions © LongBar Project Group 2010.")]
[assembly: AssemblyVersion(AssemblyInfo.SharedVersion)]

#region Assembly-level protected information classes
internal static class AssemblyInfo
{
	// The shared version of all the assemblies in this branch
	// Format: [Major.Minor.Build.Revision]
	public const string SharedVersion = "1.0.0.*";
	// The major and minor version numbers (used in about window)
	public const string SharedIV = "1.0";
}

internal static class GitInfo
{
	public const string Repository = "AvalonBar";
	public const string Milestone = "Athens";
	public const string Branch = "dev";
	public const string BranchProdName = Repository + " " + Milestone;
	public const string BranchProdStatus = "RC3";
}
#endregion