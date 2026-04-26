using System.Collections.Generic;
using System.Threading;
using aweXpect.Chronology;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Internal.Tests.Verify;

public class VerificationResultTests
{
	public sealed class AwaitableTests
	{
		[Fact]
		public async Task Verify_WhenPredicateNeverSatisfies_ShouldTimeOut()
		{
			FastMockInteractions store = new(0);
			VerificationResult<object> result = new(
				new object(),
				store,
				_ => true,
				"expected");

			VerificationResult<object> awaitable = result.Within(50.Milliseconds());

			void Act()
			{
				((IVerificationResult)awaitable).Verify(_ => false);
			}

			await That(Act).Throws<MockVerificationTimeoutException>();
		}

		[Fact]
		public async Task VerifyCount_WhenPredicateNeverSatisfies_ShouldTimeOut()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod<object>(new object(), 0, "Foo", () => "Foo()")
					.Within(50.Milliseconds()).AtLeast(2);
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*timed out*").AsWildcard();
		}

		[Fact]
		public async Task WithCancellation_PreservesUseCountAllFlag()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo", 1);
			buffer.Append("Foo", 2);
			buffer.Append("Foo", 3);

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod(
				new object(), 0, "Foo",
				(IParameterMatch<int>)It.Is(1),
				() => "Foo(1)");

			VerificationResult<object> widened = result.AnyParameters();

			void Act()
			{
				widened.WithCancellation(CancellationToken.None).Exactly(3);
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task Within_PreservesUseCountAllFlag()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo", 1);
			buffer.Append("Foo", 2);
			buffer.Append("Foo", 3);

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod(
				new object(), 0, "Foo",
				(IParameterMatch<int>)It.Is(1),
				() => "Foo(1)");

			VerificationResult<object> widened = result.AnyParameters();

			void Act()
			{
				widened.Within(50.Milliseconds()).Exactly(3);
			}

			await That(Act).DoesNotThrow();
		}
	}

	public sealed class MapTests
	{
		[Fact]
		public async Task Map_WithBuffer_PreservesFastPathSource()
		{
			FastMockInteractions store = new(1);
			FastMethod0Buffer buffer = store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo");
			buffer.Append("Foo");

			object original = new();
			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object>(
				original, 0, "Foo", () => "Foo()");

			string newSubject = "newMock";
			VerificationResult<string> mapped = result.Map(newSubject);

			await That(((IVerificationResult<string>)mapped).Object).IsEqualTo(newSubject);

			void Act()
			{
				mapped.Within(50.Milliseconds()).Exactly(2);
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task Map_WithoutBuffer_StillCarriesPredicate()
		{
			FastMockInteractions store = new(0);
			VerificationResult<object> source = new(
				new object(),
				store,
				_ => true,
				"expected");

			VerificationResult<int> mapped = source.Map(42);

			await That(((IVerificationResult<int>)mapped).Object).IsEqualTo(42);
			await That(((IVerificationResult)mapped).Expectation).IsEqualTo("expected");
		}
	}

	public sealed class CollectMatchingTests
	{
		[Fact]
		public async Task WithBufferAndMultipleRecords_PreservesSequenceOrder()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo", 10);
			buffer.Append("Foo", 20);
			buffer.Append("Foo", 30);

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object, MethodInvocation<int>>(
				new object(), 0, "Foo", _ => true, () => "Foo()");

			List<int> values = new();
			bool verified = ((IVerificationResult)result).Verify(arr =>
			{
				foreach (IInteraction interaction in arr)
				{
					values.Add(((MethodInvocation<int>)interaction).Parameter1);
				}

				return arr.Length == 3;
			});

			await That(verified).IsTrue();
			await That(values).IsEqualTo([10, 20, 30,]);
		}

		[Fact]
		public async Task WithBufferAndNoMatchingRecord_ReturnsEmpty()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo", 1);

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object, MethodInvocation<int>>(
				new object(), 0, "Foo", m => m.Parameter1 == 99, () => "Foo(99)");

			int observed = -1;
			bool verified = ((IVerificationResult)result).Verify(arr =>
			{
				observed = arr.Length;
				return arr.Length == 0;
			});

			await That(verified).IsTrue();
			await That(observed).IsEqualTo(0);
		}

		[Fact]
		public async Task WithBufferAndSingleRecord_ReturnsRecord()
		{
			FastMockInteractions store = new(1);
			FastMethod0Buffer buffer = store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			buffer.Append("Foo");

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object, IMethodInteraction>(
				new object(), 0, "Foo", _ => true, () => "Foo()");

			int observed = -1;
			bool verified = ((IVerificationResult)result).Verify(arr =>
			{
				observed = arr.Length;
				return arr.Length == 1;
			});

			await That(verified).IsTrue();
			await That(observed).IsEqualTo(1);
		}

		[Fact]
		public async Task WithEmptyBuffer_ReturnsEmpty()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object, IMethodInteraction>(
				new object(), 0, "Foo", _ => true, () => "Foo()");

			int observed = -1;
			bool verified = ((IVerificationResult)result).Verify(arr =>
			{
				observed = arr.Length;
				return arr.Length == 0;
			});

			await That(verified).IsTrue();
			await That(observed).IsEqualTo(0);
		}
	}

	public sealed class IgnoreParametersAnyParametersTests
	{
		[Fact]
		public async Task AnyParameters_WithoutBuffer_AndNoOverloadFilter_MatchesAllOfMethodName()
		{
			IMockInteractions store = new FastMockInteractions(0);

			store.RegisterInteraction(new MethodInvocation<int>("Foo", 1));
			store.RegisterInteraction(new MethodInvocation<string>("Foo", "x"));
			store.RegisterInteraction(new MethodInvocation<int>("Bar", 1));

			VerificationResult<object>.IgnoreParameters result = new(
				new object(),
				store,
				"Foo",
				_ => false,
				null,
				() => "Foo");

			void Act()
			{
				result.AnyParameters().Exactly(2);
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task AnyParameters_WithoutBuffer_KeepsOverloadFilter()
		{
			IMockInteractions store = new FastMockInteractions(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			store.RegisterInteraction(new MethodInvocation<int>("Foo", 1));
			store.RegisterInteraction(new MethodInvocation<string>("Foo", "x"));
			store.RegisterInteraction(new MethodInvocation<int>("Bar", 1));

			VerificationResult<object>.IgnoreParameters result = registry.VerifyMethod<object, MethodInvocation<int>>(
				new object(), "Foo", _ => false, () => "Foo");

			void Act()
			{
				result.AnyParameters().Once();
			}

			await That(Act).DoesNotThrow();
		}
	}
}
