namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	public sealed class CallingBaseClassTests
	{
		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyProperty_Getter_ShouldCallBaseWhenRequested(bool callBaseClass, int expectedCallCount)
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyProperty.CallingBaseClass(callBaseClass);

			_ = mock.MyProperty;

			await That(mock.MyPropertyGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyProperty_Setter_ShouldCallBaseWhenRequested(bool callBaseClass, int expectedCallCount)
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyProperty.CallingBaseClass(callBaseClass);

			mock.MyProperty = 1;

			await That(mock.MyPropertySetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Fact]
		public async Task SetupCallingBaseClassWithoutReturn_ShouldReturnBaseValue()
		{
			MyPropertyService mock = Mock.Create<MyPropertyService>();
			mock.SetupMock.Property.MyPropertyReturning2.CallingBaseClass();
			
			int result = mock.MyPropertyReturning2;

			await That(result).IsEqualTo(2);
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
