using System.Reflection;

namespace Mockolate.Tests.TestHelpers;

internal static class Helper
{
	public static MethodInfo GetMethodInfo()
		=> typeof(Helper).GetMethod(nameof(GetMethodInfo), BindingFlags.Static | BindingFlags.Public)!;
}
