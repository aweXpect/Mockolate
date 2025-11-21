namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class SkippingBaseClassTests
	{
		[Test]
		public async Task DefaultBehavior_ForRefAndOutParameter_WhenMethodNotSetup_ShouldUseDefaultValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
			int value1 = 5;

			int sum = mock.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

			await That(value1).IsEqualTo(15);
			await That(value2).IsEqualTo(30);
			await That(sum).IsEqualTo(45);
			await That(mock.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_ForRefAndOutParameter_WhenMethodSetup_ShouldUseSetupValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
			int value1 = 5;
			mock.SetupMock.Method.VirtualMethodWithRefAndOutParameters(It.IsRef<int>(x => x + 1), It.IsOut(() => 8))
				.Returns(10);

			int sum = mock.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

			await That(value1).IsEqualTo(16);
			await That(value2).IsEqualTo(8);
			await That(sum).IsEqualTo(10);
			await That(mock.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenIndexerNotSetup_ShouldInitializeIndexerValuesFromBaseClass()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

			int result = mock[4];
			mock[4] = 42;
			int result2 = mock[4];
			int result3 = mock[3];

			await That(result).IsEqualTo(8);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(6);
			await That(mock.VirtualIndexerGetterCallCount).IsEqualTo(3);
			await That(mock.VirtualIndexerSetterCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenIndexerSetup_ShouldUseSetupValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
			mock.SetupMock.Indexer(It.IsAny<int>()).Returns(15);

			int result = mock[1];
			mock[1] = 42;
			int result2 = mock[1];

			await That(result).IsEqualTo(15);
			await That(result2).IsEqualTo(15);
			await That(mock.VirtualIndexerGetterCallCount).IsEqualTo(2);
			await That(mock.VirtualIndexerSetterCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenMethodNotSetup_ShouldReturnBaseValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

			int[] value = mock.VirtualMethod();

			await That(value).IsEqualTo([4, 5,]);
			await That(mock.VirtualMethodCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenMethodSetup_ShouldReturnSetupValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
			mock.SetupMock.Method.VirtualMethod().Returns([10, 20,]);

			int[] value = mock.VirtualMethod();

			await That(value).IsEqualTo([10, 20,]);
			await That(mock.VirtualMethodCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenPropertyNotSetup_ShouldInitializePropertyWithValueFromBaseClass()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

			int result = mock.VirtualProperty;
			mock.VirtualProperty = 42;
			int result2 = mock.VirtualProperty;

			await That(result).IsEqualTo(8);
			await That(mock.VirtualPropertyValue).IsEqualTo(42);
			await That(result2).IsEqualTo(42);
			await That(mock.VirtualPropertyGetterCallCount).IsEqualTo(2);
			await That(mock.VirtualPropertySetterCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task DefaultBehavior_WhenPropertySetup_ShouldUseSetupValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
			mock.SetupMock.Property.VirtualProperty.Returns(15);

			int result = mock.VirtualProperty;
			mock.VirtualProperty = 42;
			int result2 = mock.VirtualProperty;

			await That(result).IsEqualTo(15);
			await That(mock.VirtualPropertyValue).IsEqualTo(42);
			await That(result2).IsEqualTo(15);
			await That(mock.VirtualPropertyGetterCallCount).IsEqualTo(2);
			await That(mock.VirtualPropertySetterCallCount).IsEqualTo(1);
		}

		[Test]
		public async Task
			SkippingBaseClass_ForRefAndOutParameter_WhenMethodNotSetup_ShouldSetToPreviousOrDefaultValues()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default.SkippingBaseClass());
			int value1 = 5;

			int sum = mock.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

			await That(value1).IsEqualTo(5);
			await That(value2).IsEqualTo(0);
			await That(sum).IsEqualTo(0);
			await That(mock.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(0);
		}

		[Test]
		public async Task SkippingBaseClass_ShouldNotCallIndexersOfBaseClass()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default.SkippingBaseClass());

			int result = mock[1];
			mock[1] = 2;
			int result2 = mock[1];

			await That(result).IsEqualTo(0);
			await That(result2).IsEqualTo(2);
			await That(mock.VirtualIndexerGetterCallCount).IsEqualTo(0);
			await That(mock.VirtualIndexerSetterCallCount).IsEqualTo(0);
		}

		[Test]
		public async Task SkippingBaseClass_ShouldNotCallMethodOfBaseClass()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default.SkippingBaseClass());

			_ = mock.VirtualMethod();

			await That(mock.VirtualMethodCallCount).IsEqualTo(0);
		}

		[Test]
		public async Task SkippingBaseClass_ShouldNotCallPropertiesOfBaseClass()
		{
			MyBaseClassWithVirtualCalls mock =
				Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default.SkippingBaseClass());

			int result = mock.VirtualProperty;
			mock.VirtualProperty = 42;
			int result2 = mock.VirtualProperty;

			await That(result).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(mock.VirtualPropertyGetterCallCount).IsEqualTo(0);
			await That(mock.VirtualPropertySetterCallCount).IsEqualTo(0);
		}

		public class MyBaseClassWithVirtualCalls
		{
			public int Sum { get; private set; }

			public int VirtualMethodCallCount { get; private set; }
			public int VirtualMethodWithRefAndOutParametersCallCount { get; private set; }

			public int VirtualPropertyGetterCallCount { get; private set; }
			public int VirtualPropertySetterCallCount { get; private set; }
			public int VirtualPropertyValue { get; set; } = 8;
			public int VirtualIndexerGetterCallCount { get; private set; }
			public int VirtualIndexerSetterCallCount { get; private set; }

			public virtual int VirtualProperty
			{
				get
				{
					VirtualPropertyGetterCallCount++;
					return VirtualPropertyValue;
				}
				set
				{
					VirtualPropertySetterCallCount++;
					VirtualPropertyValue = value;
				}
			}

			public virtual int this[int key]
			{
				get
				{
					VirtualIndexerGetterCallCount++;
					return key * 2;
				}
				// ReSharper disable once ValueParameterNotUsed
				set => VirtualIndexerSetterCallCount++;
			}

			public virtual int[] VirtualMethod()
			{
				VirtualMethodCallCount++;
				return [4, 5,];
			}

			public virtual int VirtualMethodWithRefAndOutParameters(ref int value1, out int value2)
			{
				VirtualMethodWithRefAndOutParametersCallCount++;
				value1 += 10;
				value2 = 2 * value1;
				return value1 + value2;
			}

			public virtual void VirtualVoidMethod(int increment) => Sum += increment;
		}
	}
}
