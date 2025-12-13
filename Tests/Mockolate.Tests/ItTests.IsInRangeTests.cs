using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsInRangeTests
	{
		[Theory]
		[InlineData(3, 5, false)]
		[InlineData(3, 6, true)]
		[InlineData(2, 4, false)]
		[InlineData(5, 5, false)]
		[InlineData(5, 6, false)]
		[InlineData(4, 6, true)]
		[InlineData(6, 7, false)]
		public async Task ExecuteWith5_Exclusive_ShouldMatchWhenRangeContains5(int minimum, int maximum,
			bool expectFound)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithInt(5);

			await That(mock.VerifyMock.Invoked.DoSomethingWithInt(It.IsInRange(minimum, maximum).Exclusive()))
				.Exactly(expectFound ? 1 : 0);
		}

		[Theory]
		[InlineData(3, 5, true)]
		[InlineData(3, 6, true)]
		[InlineData(2, 4, false)]
		[InlineData(5, 5, true)]
		[InlineData(5, 6, true)]
		[InlineData(4, 6, true)]
		[InlineData(6, 7, false)]
		public async Task ExecuteWith5_Inclusive_ShouldMatchWhenRangeContains5(int minimum, int maximum,
			bool expectFound)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			It.IInRangeParameter<int> range = It.IsInRange(minimum, maximum);
			range.Exclusive();

			mock.DoSomethingWithInt(5);

			await That(mock.VerifyMock.Invoked.DoSomethingWithInt(range.Inclusive()))
				.Exactly(expectFound ? 1 : 0);
		}

		[Theory]
		[InlineData(3, 5, true)]
		[InlineData(3, 6, true)]
		[InlineData(2, 4, false)]
		[InlineData(5, 5, true)]
		[InlineData(5, 6, true)]
		[InlineData(4, 6, true)]
		[InlineData(6, 7, false)]
		public async Task ExecuteWith5_ShouldMatchWhenRangeContains5(int minimum, int maximum, bool expectFound)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();

			mock.DoSomethingWithInt(5);

			await That(mock.VerifyMock.Invoked.DoSomethingWithInt(It.IsInRange(minimum, maximum)))
				.Exactly(expectFound ? 1 : 0);
		}

		[Fact]
		public async Task ToString_Exclusive_ShouldReturnExpectedValue()
		{
			IParameter<double> sut = It.IsInRange(1.1, 4.1).Exclusive();
			string expectedValue = "It.IsInRange(1.1, 4.1).Exclusive()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Inclusive_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = It.IsInRange(1, 4);
			string expectedValue = "It.IsInRange(1, 4)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task WithMinimumGreaterThanMaximum_ShouldThrowArgumentOutOfRangeException()
		{
			void Act()
			{
				_ = It.IsInRange(4, 3);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithParamName("maximum").And
				.WithMessage("The maximum value must be greater than or equal to the minimum value.").AsPrefix();
		}
	}
}
