using System.Reflection;
using System.Runtime.InteropServices;

// The SharedAssemblyInfo file contains parameters shared by the
// projects included in this solution. It helps in changing versions
// and branch information easily without going into every AssemblyInfo
// file with every version/milestone change.

[assembly: AssemblyCompany("The HornSide Project")]
[assembly: AssemblyProduct(GitInfo.BranchProdName)]
[assembly: AssemblyCopyright("Portions © The HornSide Project 2016. Portions © LongBar Project Group 2010.")]
[assembly: AssemblyTrademark("No trademarks are registered.")]
[assembly: AssemblyVersion(AssemblyInfo.SharedVersion)]

#region Assembly-level protected information classes
internal static class AssemblyInfo
{
	// The shared version of all the assemblies in this branch
	// Format: [Major.Minor.Build.Revision]
	public const string SharedVersion = "2.2.2000.*";
	// The major and minor version numbers (used in about window)
	public const string SharedIV = "2.2";
}

internal static class GitInfo
{
	public const string Repository = "HornSide";
	public const string Milestone = "Athens";
	public const string Branch = "unstable";
	public const string BranchProdName = Repository + " " + Milestone;
	public const string BranchProdStatus = "Alpha";
	public static bool UsingMailFeedback = false;
}
#endregion

#region Publicly accessible information about the assembly
public static class LBAssemblyInfo
{
	public static string Version = AssemblyInfo.SharedVersion;
	public static string MajorMinorVersion = AssemblyInfo.SharedIV;
	public static string GitBranch = GitInfo.Repository;
	public static string GitBranchProdName = GitInfo.BranchProdName;
	public static string Milestone = GitInfo.Milestone;
	public static string GitBranchProdStatus = GitInfo.BranchProdStatus;
}
#endregion