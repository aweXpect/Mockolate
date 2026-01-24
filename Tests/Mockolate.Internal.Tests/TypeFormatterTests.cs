using System.Linq;
using System.Reflection;
using Mockolate.Internals;

namespace Mockolate.Internal.Tests;

public sealed class TypeFormatterTests
{
	[Fact]
	public async Task GenericArguments_ShouldFormatToName()
	{
		MethodInfo methodInfo = GetType().GetMethod(nameof(MyMethod), BindingFlags.NonPublic | BindingFlags.Static)!;
		Type type = methodInfo.GetGenericArguments().First();

		string result = type.FormatType();

		await That(result).IsEqualTo("T");
	}

	[Fact]
	public async Task NestedType_ShouldIncludeDeclaringType()
	{
		Type type = typeof(MyType);

		string result = type.FormatType();

		await That(result).IsEqualTo("TypeFormatterTests.MyType");
	}

	private class MyType;

	// ReSharper disable once UnusedTypeParameter
	private static void MyMethod<T>() { }
}
