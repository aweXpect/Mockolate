using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Internal.Tests.Verify;

public class TypedVerifyFastPathTests
{
	[Fact]
	public async Task VerifyMethod0_TypedFastPath_ShouldCount()
	{
		FastMockInteractions store = new(1);
		FastMethod0Buffer buffer = store.InstallMethod(0);

		MockRegistry registry = new(MockBehavior.Default, store);

		buffer.Append("Foo");
		buffer.Append("Foo");

		registry.VerifyMethod<object>(new object(), 0, "Foo", () => "Foo()").Twice();

		await That(true).IsTrue();
	}

	[Fact]
	public async Task VerifyMethod1_TypedFastPath_FailsWithExpectedMessage()
	{
		FastMockInteractions store = new(1);
		FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

		MockRegistry registry = new(MockBehavior.Default, store);

		buffer.Append("Foo", 1);
		buffer.Append("Foo", 1);

		await That(() => registry.VerifyMethod<object, int>(new object(), 0, "Foo",
				(IParameterMatch<int>)It.Is(1), () => "Foo(1)").Once())
			.Throws<MockVerificationException>();
	}

	[Fact]
	public async Task VerifyMethod1_TypedFastPath_ShouldHonorMatcher()
	{
		FastMockInteractions store = new(1);
		FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

		MockRegistry registry = new(MockBehavior.Default, store);

		buffer.Append("Foo", 1);
		buffer.Append("Foo", 2);
		buffer.Append("Foo", 1);

		registry.VerifyMethod(new object(), 0, "Foo",
			(IParameterMatch<int>)It.Is(1), () => "Foo(1)").Twice();
		registry.VerifyMethod<object, int>(new object(), 0, "Foo",
			(IParameterMatch<int>)It.IsAny<int>(), () => "Foo(*)").Exactly(3);

		await That(true).IsTrue();
	}

	[Fact]
	public async Task VerifyMethod2_TypedFastPath_AnyParameters_UsesCountAll()
	{
		FastMockInteractions store = new(1);
		FastMethod2Buffer<int, string> buffer = store.InstallMethod<int, string>(0);

		MockRegistry registry = new(MockBehavior.Default, store);

		buffer.Append("Foo", 1, "a");
		buffer.Append("Foo", 2, "b");
		buffer.Append("Foo", 3, "c");

		// Without AnyParameters: only matches values where m1==1
		VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod(
			new object(), 0, "Foo",
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("z"), // never matches
			() => "Foo(1, z)");

		// AnyParameters → CountAll → all 3 match
		result.AnyParameters().Exactly(3);

		await That(true).IsTrue();
	}
}
