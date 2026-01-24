using System.Collections.Generic;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public class CallbackTests
{
	[Fact]
	public async Task Invoke_ShouldIncludeIndexWhenMatching()
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
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(4, true)]
	[InlineData(5, false)]
	public async Task Invoke_WithForAndWhen_ShouldMatchInExpectedIterations(int invocationCount, bool expectResult)
	{
		Callback<Action> sut = new(() => { });
		sut.For(2);
		sut.When(v => v > 1);

		int index = 0;
		bool result = false;
		for (int iteration = 1; iteration <= invocationCount; iteration++)
		{
			result = sut.Invoke(ref index, (_, _) => { });
		}

		await That(result).IsEqualTo(expectResult);
	}

	[Fact]
	public async Task Invoke_WithReturnValue_ShouldIncludeIndexWhenMatching()
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
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(4, true)]
	[InlineData(5, false)]
	public async Task Invoke_WithReturnValue_WithForAndWhen_ShouldMatchInExpectedIterations(int invocationCount,
		bool expectResult)
	{
		Callback<Action> sut = new(() => { });
		sut.For(2);
		sut.When(v => v > 1);

		int index = 0;
		bool result = false;
		for (int iteration = 1; iteration <= invocationCount; iteration++)
		{
			result = sut.Invoke(ref index, (_, _) => "foo", out string? _);
		}

		await That(result).IsEqualTo(expectResult);
	}
}
