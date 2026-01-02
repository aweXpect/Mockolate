using Mockolate.Exceptions;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class RegisterTests
	{
		[Fact]
		public async Task AccessWithoutSetup_ShouldThrowMockNotSetupException()
		{
			IPropertyService mock = Mock.Create<IPropertyService>(MockBehavior.Default.ThrowingWhenNotSetup());

			void Act()
			{
				mock.MyStringProperty = "1";
			}

			await That(Act).Throws<MockNotSetupException>()
				.WithMessage(
					"The property 'Mockolate.Tests.MockProperties.SetupPropertyTests.IPropertyService.MyStringProperty' was accessed without prior setup.");
		}

		[Fact]
		public async Task RegisterBeforeAccess_ShouldNotThrowAndReturnDefaultValue()
		{
			IPropertyService mock = Mock.Create<IPropertyService>(MockBehavior.Default.ThrowingWhenNotSetup());

			mock.SetupMock.Property.MyStringProperty.Register();

			string? initialResult = mock.MyStringProperty;
			mock.MyStringProperty = "foo";
			string? otherResult = mock.MyStringProperty;

			await That(initialResult).IsEmpty();
			await That(otherResult).IsEqualTo("foo");
		}
	}
}
