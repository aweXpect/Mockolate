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

		public class MyPropertyService
		{
			public int MyPropertyGetterCallCount { get; private set; }
			public int MyPropertySetterCallCount { get; private set; }

			public virtual int MyProperty
			{
				get => MyPropertyGetterCallCount++;
				set => MyPropertySetterCallCount += value;
			}
		}
	}
}
