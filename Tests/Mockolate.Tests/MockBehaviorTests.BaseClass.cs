namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task WithDefaultBehavior_ForRefAndOutParameter_WhenMethodNotSetup_ShouldSetToPreviousOrDefaultValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
		int value1 = 5;

		int sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(5);
		await That(value2).IsEqualTo(0);
		await That(sum).IsEqualTo(0);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallIndexersOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		int result = mock.Subject[1];
		mock.Subject[1] = 2;
		int result2 = mock.Subject[1];

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(0);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallMethodOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		int[] result = mock.Subject.VirtualMethod();

		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallPropertiesOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		int result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		int result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(0);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenMethodNotSetup_ShouldUseBaseClassValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});
		int value1 = 5;

		int sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(15);
		await That(value2).IsEqualTo(0);
		await That(sum).IsEqualTo(0);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenMethodSetup_ShouldUseSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});
		int value1 = 5;
		mock.Setup.Method.VirtualMethodWithRefAndOutParameters(With.Ref<int>(x => x + 1), With.Out(() => 8))
			.Returns(10);

		int sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(16);
		await That(value2).IsEqualTo(8);
		await That(sum).IsEqualTo(10);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallIndexersOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});

		int result = mock.Subject[1];
		mock.Subject[1] = 42;
		int result2 = mock.Subject[1];

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallMethodsOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});

		mock.Subject.VirtualVoidMethod(2);
		mock.Subject.VirtualVoidMethod(4);
		int result = mock.Subject.Sum;

		await That(result).IsEqualTo(6);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallPropertiesOfBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});

		int result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		int result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(0);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenMethodNotSetup_ShouldReturnDefaultValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});

		int[] value = mock.Subject.VirtualMethod();

		await That(value).IsEmpty();
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenMethodSetup_ShouldReturnSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass,
		});
		mock.Setup.Method.VirtualMethod().Returns([10, 20,]);

		int[] value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20,]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenMethodNotSetup_ShouldUseBaseClassValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});
		int value1 = 5;

		int sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(15);
		await That(value2).IsEqualTo(30);
		await That(sum).IsEqualTo(45);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenMethodSetup_ShouldUseSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});
		int value1 = 5;
		mock.Setup.Method.VirtualMethodWithRefAndOutParameters(With.Ref<int>(x => x + 1), With.Out(() => 8))
			.Returns(10);

		int sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(16);
		await That(value2).IsEqualTo(8);
		await That(sum).IsEqualTo(10);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenIndexerNotSetup_ShouldInitializeIndexerValuesFromBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});

		int result = mock.Subject[4];
		mock.Subject[4] = 42;
		int result2 = mock.Subject[4];
		int result3 = mock.Subject[3];

		await That(result).IsEqualTo(8);
		await That(result2).IsEqualTo(42);
		await That(result3).IsEqualTo(6);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(3);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenIndexerSetup_ShouldUseSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});
		mock.Setup.Indexer(With.Any<int>()).Returns(15);

		int result = mock.Subject[1];
		mock.Subject[1] = 42;
		int result2 = mock.Subject[1];

		await That(result).IsEqualTo(15);
		await That(result2).IsEqualTo(15);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenMethodNotSetup_ShouldReturnBaseValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});

		int[] value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([4, 5,]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenMethodSetup_ShouldReturnSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});
		mock.Setup.Method.VirtualMethod().Returns([10, 20,]);

		int[] value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20,]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task
		WithUseBaseClassAsDefaultValue_WhenPropertyNotSetup_ShouldInitializePropertyWithValueFromBaseClass()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});

		int result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		int result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(8);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenPropertySetup_ShouldUseSetupValues()
	{
		Mock<MyBaseClassWithVirtualCalls> mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
		});
		mock.Setup.Property.VirtualProperty.Returns(15);

		int result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		int result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(15);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(15);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
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
