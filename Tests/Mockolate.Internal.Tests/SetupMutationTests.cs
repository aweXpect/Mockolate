using System.Collections.Generic;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public class SetupMutationTests
{
	public sealed class MethodSetupsFindTests
	{
		[Fact]
		public async Task GetLatestOrDefault_WithSingleMatchingSetup_ShouldReturnIt()
		{
			MockSetups.MethodSetups setups = new();
			FakeMethodSetup setup = new();
			setups.Add(setup);

			MethodSetup? result = setups.GetLatestOrDefault(_ => true);

			await That(result).IsSameAs(setup);
		}
	}

	public sealed class PropertySetupsCountTests
	{
		[Fact]
		public async Task Add_ReplacingDefaultWithUserSetup_ShouldIncrementCountByOne()
		{
			MockSetups.PropertySetups setups = new();
			PropertySetup defaultSetup = new PropertySetup.Default<int>("p", 0);
			FakePropertySetup userSetup = new("p");

			setups.Add(defaultSetup);
			await That(setups.Count).IsEqualTo(0);

			setups.Add(userSetup);

			await That(setups.Count).IsEqualTo(1);
			setups.TryGetValue("p", out PropertySetup? found);
			await That(found).IsSameAs(userSetup);
		}
	}

	public sealed class StatePassingInvokeTests
	{
		[Fact]
		public async Task ShouldIncludeIndexWhenMatching()
		{
			bool wasInvoked = false;
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.Only(2);
			sut.When(v => v > 1);

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke(wasInvoked, ref index, values, static (v, _, list) => list.Add(v));
			}

			await That(values).IsEqualTo([2, 3,]);
		}

		[Fact]
		public async Task ShouldLimitExecutionWhenRunningInParallel()
		{
			bool wasInvoked = false;
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.Only(2);
			sut.InParallel();

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke(wasInvoked, ref index, values, static (v, _, list) => list.Add(v));
			}

			await That(values).IsEqualTo([0, 1,]);
		}

		[Theory]
		[InlineData(2, 2, 0, 1, 1, 2, 2, 2, 2, 2, 2, 2)]
		[InlineData(2, 3, 0, 1, 1, 2, 2, 3, 3, 3, 3, 3)]
		public async Task ShouldIncrementIndexWheneverForIsExhausted(
			int @for, int only, params int[] expectResult)
		{
			bool wasInvoked = false;
			List<int> indexValues = [];
			Callback<Action> sut = new(() => { });
			sut.For(@for);
			sut.Only(only);

			int index = 0;
			for (int iteration = 1; iteration <= 10; iteration++)
			{
				sut.Invoke(wasInvoked, ref index, 0, static (_, _, _) => { });
				indexValues.Add(index);
			}

			await That(indexValues).IsEqualTo(expectResult);
		}
	}

	public sealed class StatePassingReturnInvokeTests
	{
		[Fact]
		public async Task ShouldIncludeIndexWhenMatching()
		{
			List<int> values = [];
			Callback<Action> sut = new(() => { });
			sut.Only(2);
			sut.When(v => v > 1);

			int index = 0;
			for (int i = 0; i < 5; i++)
			{
				sut.Invoke<List<int>, string>(ref index, values,
					static (v, _, list) =>
					{
						list.Add(v);
						return "";
					}, out _);
			}

			await That(values).IsEqualTo([2, 3,]);
		}
	}
}
