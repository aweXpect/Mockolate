using System.Collections.Generic;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public class CallbackTests
{
	public sealed class InvokeForCallbacksTests
	{
		[Fact]
		public async Task ShouldIncludeIndexWhenMatching()
		{
			bool wasInvoked = false;
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke(wasInvoked, ref index, (v, _) => values.Add(v));
			}

			await That(values).IsEqualTo([2, 3,]);
		}

		[Theory]
		[InlineData(2, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1)]
		[InlineData(2, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1)]
		public async Task ShouldIncrementIndexOnceWhenCallbackIsExhausted(
			int forValue, int whenMinimum, params int[] expectResult)
		{
			bool wasInvoked = false;
			List<int> indexValues = [];
			Callback<Action> sut = new(() => { });
			sut.For(forValue);
			sut.When(v => v > whenMinimum);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(wasInvoked, ref index, (_, _) => { });
				indexValues.Add(index);
			}

			await That(indexValues).IsEqualTo(expectResult);
		}

		[Fact]
		public async Task WhenCheck_ShouldOnlyBeCalledWhenOnlyLimitIsNotReached()
		{
			bool wasInvoked = false;
			int[] expectResult = [0, 1, 2, 3, 4, 5,];
			List<int> invocationChecks = [];
			int invocationCount = 0;
			Callback<Action> sut = new(() => { invocationCount++; });
			sut.For(2);
			sut.When(v =>
			{
				invocationChecks.Add(v);
				return true;
			});
			sut.Only(4);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(wasInvoked, ref index, (_, c) => c());
			}

			await That(invocationChecks).IsEqualTo(expectResult);
			await That(invocationCount).IsEqualTo(2);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(2, false)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		[InlineData(5, false)]
		public async Task WithForAndWhen_ShouldMatchInExpectedIterations(
			int iterations, bool expectResult)
		{
			bool wasInvoked = false;
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			bool result = false;
			for (int iteration = 1; iteration <= iterations; iteration++)
			{
				result = sut.Invoke(wasInvoked, ref index, (_, _) => { });
			}

			await That(result).IsEqualTo(expectResult);
		}
	}

	public sealed class InvokeForVoidThrowsTests
	{
		[Fact]
		public async Task ShouldIncludeIndexWhenMatching()
		{
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke(ref index, (v, _) => values.Add(v));
			}

			await That(values).IsEqualTo([2, 3,]);
		}

		[Theory]
		[InlineData(2, 0, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9)]
		[InlineData(2, 1, 1, 2, 2, 3, 4, 5, 6, 7, 8, 9)]
		[InlineData(3, 1, 1, 2, 2, 2, 3, 4, 5, 6, 7, 8)]
		[InlineData(5, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 6)]
		[InlineData(6, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 5)]
		[InlineData(7, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 5)]
		[InlineData(3, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
		public async Task ShouldIncrementIndexWhenForIsNotActive(
			int forValue, int whenMinimum, params int[] expectResult)
		{
			List<int> indexValues = [];
			int invocationCount = 0;
			int expectedInvocationCount = Math.Max(0, Math.Min(forValue, 9 - whenMinimum));
			Callback<Action> sut = new(() => { invocationCount++; });
			sut.For(forValue);
			sut.When(v => v > whenMinimum);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(ref index, (_, c) => c());
				indexValues.Add(index);
			}

			await That(indexValues).IsEqualTo(expectResult);
			await That(invocationCount).IsEqualTo(expectedInvocationCount);
		}

		[Fact]
		public async Task WhenCheck_ShouldOnlyBeCalledWhenOnlyLimitIsNotReached()
		{
			int[] expectResult = [0, 1, 2, 3, 4, 5,];
			List<int> invocationChecks = [];
			int invocationCount = 0;
			Callback<Action> sut = new(() => { invocationCount++; });
			sut.For(2);
			sut.When(v =>
			{
				invocationChecks.Add(v);
				return true;
			});
			sut.Only(4);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(ref index, (_, c) => c());
			}

			await That(invocationChecks).IsEqualTo(expectResult);
			await That(invocationCount).IsEqualTo(2);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(2, false)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		[InlineData(5, false)]
		public async Task WithForAndWhen_ShouldMatchInExpectedIterations(
			int iterations, bool expectResult)
		{
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			bool result = false;
			for (int iteration = 1; iteration <= iterations; iteration++)
			{
				result = sut.Invoke(ref index, (_, _) => { });
			}

			await That(result).IsEqualTo(expectResult);
		}
	}

	public sealed class InvokeForReturnThrowsTests
	{
		[Fact]
		public async Task ShouldIncludeIndexWhenMatching()
		{
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke<string>(ref index, (v, _) =>
				{
					values.Add(v);
					return $"foo-{v}";
				}, out string? _);
			}

			await That(values).IsEqualTo([2, 3,]);
		}

		[Theory]
		[InlineData(2, 0, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9)]
		[InlineData(2, 1, 1, 2, 2, 3, 4, 5, 6, 7, 8, 9)]
		[InlineData(3, 1, 1, 2, 2, 2, 3, 4, 5, 6, 7, 8)]
		[InlineData(5, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 6)]
		[InlineData(6, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 5)]
		[InlineData(7, 4, 1, 2, 3, 4, 5, 5, 5, 5, 5, 5)]
		[InlineData(3, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
		public async Task ShouldIncrementIndexWhenForIsNotActive(
			int forValue, int whenMinimum, params int[] expectResult)
		{
			List<int> indexValues = [];
			int invocationCount = 0;
			int expectedInvocationCount = Math.Max(0, Math.Min(forValue, 9 - whenMinimum));
			Callback<Action> sut = new(() => { invocationCount++; });
			sut.For(forValue);
			sut.When(v => v > whenMinimum);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(ref index, (_, c) =>
				{
					c();
					return "foo";
				}, out string? _);
				indexValues.Add(index);
			}

			await That(indexValues).IsEqualTo(expectResult);
			await That(invocationCount).IsEqualTo(expectedInvocationCount);
		}

		[Fact]
		public async Task WhenCheck_ShouldOnlyBeCalledWhenOnlyLimitIsNotReached()
		{
			int[] expectResult = [0, 1, 2, 3, 4, 5,];
			List<int> invocationChecks = [];
			int invocationCount = 0;
			Callback<Action> sut = new(() => { invocationCount++; });
			sut.For(2);
			sut.When(v =>
			{
				invocationChecks.Add(v);
				return true;
			});
			sut.Only(4);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(ref index, (_, c) =>
				{
					c();
					return "foo";
				}, out string? _);
			}

			await That(invocationChecks).IsEqualTo(expectResult);
			await That(invocationCount).IsEqualTo(2);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(2, false)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		[InlineData(5, false)]
		public async Task WithForAndWhen_ShouldMatchInExpectedIterations(
			int iterations, bool expectResult)
		{
			Callback<Action> sut = new(() => { });
			sut.For(2);
			sut.When(v => v > 1);

			int index = 0;
			bool result = false;
			for (int iteration = 1; iteration <= iterations; iteration++)
			{
				result = sut.Invoke(ref index, (_, _) => "foo", out string? _);
			}

			await That(result).IsEqualTo(expectResult);
		}
	}
}
