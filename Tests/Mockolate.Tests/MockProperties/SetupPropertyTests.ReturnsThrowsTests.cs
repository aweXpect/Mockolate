namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class ReturnsThrowsTests
	{
		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Subject.MyProperty;
			Exception? result2 = Record.Exception(() => _ = sut.Subject.MyProperty);
			int result3 = sut.Subject.MyProperty;

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Returns(4)
				.Returns(() => 3)
				.Returns(v => 10 * v);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.MyProperty;
			}

			await That(result).IsEqualTo([4, 3, 30, 4, 3, 30, 4, 3, 30, 4,]);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Returns(() => 4);

			int result = sut.Subject.MyProperty;

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.InitializeWith(3)
				.Returns(x => 4 * x);

			int result = sut.Subject.MyProperty;

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Returns(4);

			int result = sut.Subject.MyProperty;

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			int result = sut.Subject.MyProperty;

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task Throws_Callback_ShouldThrowException()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Throws(() => new Exception("foo"));

			void Act()
				=> _ = sut.Subject.MyProperty;

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldThrowException()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.InitializeWith(42)
				.Throws(v => new Exception($"foo-{v}"));

			void Act()
				=> _ = sut.Subject.MyProperty;

			await That(Act).ThrowsException().WithMessage("foo-42");
		}

		[Fact]
		public async Task Throws_ShouldThrowException()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Throws(new Exception("foo"));

			void Act()
				=> _ = sut.Subject.MyProperty;

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_Generic_ShouldThrowException()
		{
			Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

			sut.Setup.Property.MyProperty
				.Throws<ArgumentNullException>();

			void Act()
				=> _ = sut.Subject.MyProperty;

			await That(Act).ThrowsExactly<ArgumentNullException>();
		}
	}
}
