namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class SkippingBaseClassTests
	{
		[Test]
		[Arguments(false, 1)]
		[Arguments(true, 0)]
		public async Task MyProperty_Getter_ShouldSkipCallingBaseWhenRequested(bool skipBaseClass, int expectedCallCount)
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyProperty.SkippingBaseClass(skipBaseClass);

			_ = mock.MyProperty;

			await That(mock.MyPropertyGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Test]
		[Arguments(false, 1)]
		[Arguments(true, 0)]
		public async Task MyProperty_Setter_ShouldSkipCallingBaseWhenRequested(bool skipBaseClass, int expectedCallCount)
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyProperty.SkippingBaseClass(skipBaseClass);

			mock.MyProperty = 1;

			await That(mock.MyPropertySetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Test]
		public async Task SetupSkippingBaseClassWithoutParameter_ShouldReturnDefaultValue()
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyPropertyReturning2.SkippingBaseClass();

			int result = mock.MyPropertyReturning2;

			await That(result).IsEqualTo(0);
		}

		public class MyPropertyService
		{
			public int MyPropertyGetterCallCount { get; private set; }
			public int MyPropertySetterCallCount { get; private set; }

			public virtual int MyProperty
			{
				get => MyPropertyGetterCallCount++;
				set => MyPropertySetterCallCount += value;
			}

			public virtual int MyPropertyReturning2 => 2;
		}
	}
}
