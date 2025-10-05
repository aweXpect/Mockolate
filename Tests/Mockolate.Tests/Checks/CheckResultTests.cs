using System.Linq;
using Mockolate.Checks;
using Mockolate.Checks.Interactions;

namespace Mockolate.Tests.Checks;

public class CheckResultTests
{
	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, true)]
	public async Task AtLeast_ShouldReturnExpectedResult(int count, int times, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.AtLeast(times);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastOnce_ShouldReturnExpectedResult(int count, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.AtLeastOnce();

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 1, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 3, true)]
	public async Task AtMost_ShouldReturnExpectedResult(int count, int times, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.AtMost(times);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task AtMostOnce_ShouldReturnExpectedResult(int count, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.AtMostOnce();

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, false)]
	public async Task Exactly_ShouldReturnExpectedResult(int count, int times, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.Exactly(times);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Never_ShouldReturnExpectedResult(int count, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.Never();

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Once_ShouldReturnExpectedResult(int count, bool expectedResult)
	{
		MyInteraction[] interactions = Enumerable.Range(0, count)
			.Select(_ => new MyInteraction())
			.ToArray();
		CheckResult sut = new(new Mockolate.Checks.Checks(), interactions);

		bool result = sut.Once();

		await That(result).IsEqualTo(expectedResult);
	}

	private class MyInteraction : IInteraction
	{
	}
}
