using System.Reflection;

namespace Mockolate.Tests.TestHelpers;

internal static class Helper
{
	public static MethodInfo GetMethodInfo()
	{
		return typeof(Helper).GetMethod(nameof(GetMethodInfo), BindingFlags.Static | BindingFlags.Public)!;
	}
}
