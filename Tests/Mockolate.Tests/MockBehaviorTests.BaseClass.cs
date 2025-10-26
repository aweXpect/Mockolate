using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mockolate.DefaultValues;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenMethodNotSetup_ShouldReturnBaseValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([4, 5]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenMethodSetup_ShouldReturnSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		mock.Setup.Method.VirtualMethod().Returns([10, 20]);

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenMethodNotSetup_ShouldReturnDefaultValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEmpty();
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenMethodSetup_ShouldReturnSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });
		mock.Setup.Method.VirtualMethod().Returns([10, 20]);

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenMethodNotSetup_ShouldUseBaseClassValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });
		int value1 = 5;

		var sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(15);
		await That(value2).IsEqualTo(0);
		await That(sum).IsEqualTo(0);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithDefaultBehavior_ForRefAndOutParameter_WhenMethodNotSetup_ShouldSetToPreviousOrDefaultValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);
		int value1 = 5;

		var sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(5);
		await That(value2).IsEqualTo(0);
		await That(sum).IsEqualTo(0);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenMethodSetup_ShouldUseSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });
		int value1 = 5;
		mock.Setup.Method.VirtualMethodWithRefAndOutParameters(With.Ref<int>(x => x + 1), With.Out<int>(() => 8)).Returns(10);

		var sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(16);
		await That(value2).IsEqualTo(8);
		await That(sum).IsEqualTo(10);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenMethodNotSetup_ShouldUseBaseClassValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		int value1 = 5;

		var sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(15);
		await That(value2).IsEqualTo(30);
		await That(sum).IsEqualTo(45);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenMethodSetup_ShouldUseSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		int value1 = 5;
		mock.Setup.Method.VirtualMethodWithRefAndOutParameters(With.Ref<int>(x => x + 1), With.Out<int>(() => 8)).Returns(10);

		var sum = mock.Subject.VirtualMethodWithRefAndOutParameters(ref value1, out int value2);

		await That(value1).IsEqualTo(16);
		await That(value2).IsEqualTo(8);
		await That(sum).IsEqualTo(10);
		await That(mock.Subject.VirtualMethodWithRefAndOutParametersCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallMethodsOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		mock.Subject.VirtualVoidMethod(2);
		mock.Subject.VirtualVoidMethod(4);
		var result = mock.Subject.Sum;

		await That(result).IsEqualTo(6);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallPropertiesOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		var result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		var result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(0);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ShouldCallIndexersOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		var result = mock.Subject[1];
		mock.Subject[1] = 42;
		var result2 = mock.Subject[1];

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallMethodOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		var result = mock.Subject.VirtualMethod();

		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallPropertiesOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		var result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		var result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(0);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithDefaultBehavior_ShouldNotCallIndexersOfBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default);

		var result = mock.Subject[1];
		mock.Subject[1] = 2;
		var result2 = mock.Subject[1];

		await That(result).IsEqualTo(0);
		await That(result2).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(0);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenPropertyNotSetup_ShouldInitializePropertyWithValueFromBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });

		var result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		var result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(8);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(42);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenPropertySetup_ShouldUseSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		mock.Setup.Property.VirtualProperty.Returns(15);

		var result = mock.Subject.VirtualProperty;
		mock.Subject.VirtualProperty = 42;
		var result2 = mock.Subject.VirtualProperty;

		await That(result).IsEqualTo(15);
		await That(mock.Subject.VirtualPropertyValue).IsEqualTo(42);
		await That(result2).IsEqualTo(15);
		await That(mock.Subject.VirtualPropertyGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualPropertySetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenIndexerNotSetup_ShouldInitializeIndexerValuesFromBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });

		var result = mock.Subject[4];
		mock.Subject[4] = 42;
		var result2 = mock.Subject[4];
		var result3 = mock.Subject[3];

		await That(result).IsEqualTo(8);
		await That(result2).IsEqualTo(42);
		await That(result3).IsEqualTo(6);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(3);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenIndexerSetup_ShouldUseSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		mock.Setup.Indexer(With.Any<int>()).Returns(15);

		var result = mock.Subject[1];
		mock.Subject[1] = 42;
		var result2 = mock.Subject[1];

		await That(result).IsEqualTo(15);
		await That(result2).IsEqualTo(15);
		await That(mock.Subject.VirtualIndexerGetterCallCount).IsEqualTo(2);
		await That(mock.Subject.VirtualIndexerSetterCallCount).IsEqualTo(1);
	}

	public class MyBaseClassWithVirtualCalls
	{
		private int _sum;
		public int Sum => _sum;

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
			set
			{
				VirtualIndexerSetterCallCount++;
			}
		}

		public virtual int[] VirtualMethod()
		{
			VirtualMethodCallCount++;
			return [4, 5];
		}

		public virtual int VirtualMethodWithRefAndOutParameters(ref int value1, out int value2)
		{
			VirtualMethodWithRefAndOutParametersCallCount++;
			value1 += 10;
			value2 = 2 * value1;
			return value1 + value2;
		}

		public virtual void VirtualVoidMethod(int increment)
		{
			_sum += increment;
		}
	}
}
