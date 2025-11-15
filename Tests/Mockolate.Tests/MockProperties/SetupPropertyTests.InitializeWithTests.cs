namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class InitializeWithTests
	{
		[Fact]
		public async Task WhenRead_ShouldReturnInitializedValue()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty.InitializeWith(42);

			int result = sut.MyProperty;

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task WhenSet_ShouldUpdateValue_ShouldReturnInitializedValue()
		{
			IPropertyService sut = Mock.Create<IPropertyService>();
			sut.SetupMock.Property.MyProperty.InitializeWith(42);

			int result1 = sut.MyProperty;
			sut.MyProperty = 100;
			int result2 = sut.MyProperty;

			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(100);
		}
	}
}
