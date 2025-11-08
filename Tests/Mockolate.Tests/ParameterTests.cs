using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	[Fact]
	public async Task ShouldOnlyHaveOneParameterlessPrivateConstructor()
	{
		ConstructorInfo[] constructors = typeof(Parameter)
			.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

		await That(constructors).HasSingle().Which
			.For(c => c.GetParameters(), p => p.HasCount(0));

		_ = constructors.Single().Invoke([]);
	}

	[Fact]
	public async Task ToString_NamedParameter_ShouldReturnExpectedValue()
	{
		NamedParameter sut = new("foo", Out<int>());
		string expectedValue = "Out<int>() foo";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedValue);
	}

	internal interface IMyServiceWithNullable
	{
		void DoSomething(int? value, bool flag);
	}

	internal class AllEqualComparer : IEqualityComparer<int>
	{
		bool IEqualityComparer<int>.Equals(int x, int y) => true;

		int IEqualityComparer<int>.GetHashCode(int obj) => obj.GetHashCode();
	}
}
