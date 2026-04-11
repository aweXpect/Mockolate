using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	[Fact]
	public async Task InvokeCallbacks_WithCorrectType_ShouldInvokeCallback()
	{
		int isCalled = 0;
		IParameter<int> sut = It.Satisfies<int>(_ => true)
			.Do(v => isCalled += v);

		((IParameterMatch<int>)sut).InvokeCallbacks(5);

		await That(isCalled).IsEqualTo(5);
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

	internal interface IMyServiceWithNullable
	{
		void DoSomething(int? value, bool flag);
		void DoSomethingWithInt(int value);
		void DoSomethingWithLong(int value);
		void DoSomethingWithString(string value);
	}

	internal class AllEqualComparer : IEqualityComparer<int>
	{
		bool IEqualityComparer<int>.Equals(int x, int y) => true;

		int IEqualityComparer<int>.GetHashCode(int obj) => obj.GetHashCode();
	}
}
