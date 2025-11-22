using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	[Fact]
	public async Task InvokeCallbacks_WithCorrectType_ShouldInvokeCallback()
	{
		int isCalled = 0;
		IParameter<int> sut = With<int>(_ => true)
			.Do(v => isCalled += v);

		((IParameter)sut).InvokeCallbacks(5);

		await That(isCalled).IsEqualTo(5);
	}

	[Fact]
	public async Task InvokeCallbacks_WithDifferentType_ShouldNotInvokeCallback()
	{
		int isCalled = 0;
		IParameter<int> sut = With<int>(_ => true)
			.Do(v => isCalled += v);

		((IParameter)sut).InvokeCallbacks("5");

		await That(isCalled).IsEqualTo(0);
	}

	[Fact]
	public async Task InvokeCallbacks_WithNull_WhenTypeIsNotNullable_ShouldNotInvokeCallback()
	{
		int isCalled = 0;
		IParameter<int> sut = With<int>(_ => true)
			.Do(_ => isCalled++);

		((IParameter)sut).InvokeCallbacks(null);

		await That(isCalled).IsEqualTo(0);
	}

	[Fact]
	public async Task InvokeCallbacks_WithNull_WhenTypeIsNullable_ShouldInvokeCallback()
	{
		int isCalled = 0;
		IParameter<int?> sut = With<int?>(_ => true)
			.Do(_ => isCalled++);

		((IParameter)sut).InvokeCallbacks(null);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task ShouldOnlyHaveOneParameterlessPrivateConstructor()
	{
		ConstructorInfo[] constructors = typeof(Match)
			.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

		await That(constructors).HasSingle().Which
			.For(c => c.GetParameters(), p => p.HasCount(0));

		_ = constructors.Single().Invoke([]);
	}

	[Fact]
	public async Task ToString_NamedParameter_ShouldReturnExpectedValue()
	{
		NamedParameter sut = new("foo", (IParameter)Out<int>());
		string expectedValue = "Out<int>() foo";

		string result = sut.ToString();

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
