namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class ReturnsThrowsTests
	{
		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.MyProperty;
			Exception? result2 = Record.Exception(() => _ = sut.MyProperty);
			int result3 = sut.MyProperty;

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Returns(4)
				.Returns(() => 3)
				.Returns(v => 10 * v);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.MyProperty;
			}

			await That(result).IsEqualTo([4, 3, 30, 4, 3, 30, 4, 3, 30, 4,]);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Returns(() => 4);

			int result = sut.MyProperty;

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(3)
				.Returns(x => 4 * x);

			int result = sut.MyProperty;

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Returns(4);

			int result = sut.MyProperty;

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			int result = sut.MyProperty;

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task Throws_Callback_ShouldThrowException()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Throws(() => new Exception("foo"));

			void Act()
			{
				_ = sut.MyProperty;
			}

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldThrowException()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.InitializeWith(42)
				.Throws(v => new Exception($"foo-{v}"));

			void Act()
			{
				_ = sut.MyProperty;
			}

			await That(Act).ThrowsException().WithMessage("foo-42");
		}

		[Fact]
		public async Task Throws_Generic_ShouldThrowException()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Throws<ArgumentNullException>();

			void Act()
			{
				_ = sut.MyProperty;
			}

			await That(Act).ThrowsExactly<ArgumentNullException>();
		}

		[Fact]
		public async Task Throws_ShouldThrowException()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();

			sut.SetupMock.Property.MyProperty
				.Throws(new Exception("foo"));

			void Act()
			{
				_ = sut.MyProperty;
			}

			await That(Act).ThrowsException().WithMessage("foo");
		}
	}
}
