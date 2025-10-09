using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mockolate.Tests.Checks;

namespace Mockolate.Tests.TestHelpers;

internal static class Helper
{
	public static MethodInfo GetMethodInfo()
	{
		return typeof(Helper).GetMethod(nameof(GetMethodInfo), BindingFlags.Static | BindingFlags.Public)!;
	}
}
