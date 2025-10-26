using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mockolate.DefaultValues;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenNotSetup_ShouldReturnBaseValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([4, 5]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithUseBaseClassAsDefaultValue_WhenSetup_ShouldReturnSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue });
		mock.Setup.Method.VirtualMethod().Returns([10, 20]);

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenNotSetup_ShouldReturnDefaultValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEmpty();
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_WhenSetup_ShouldReturnSetupValues()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });
		mock.Setup.Method.VirtualMethod().Returns([10, 20]);

		var value = mock.Subject.VirtualMethod();

		await That(value).IsEqualTo([10, 20]);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenNotSetup_ShouldUseBaseClassValues()
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
	public async Task WithDefaultBehavior_ForRefAndOutParameter_WhenNotSetup_ShouldSetToPreviousOrDefaultValues()
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
	public async Task WithOnlyCallBaseClass_ForRefAndOutParameter_WhenSetup_ShouldUseSetupValues()
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
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenNotSetup_ShouldUseBaseClassValues()
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
	public async Task WithUseBaseClassAsDefaultValue_ForRefAndOutParameter_WhenSetup_ShouldUseSetupValues()
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
	public async Task WithOnlyCallBaseClass_ShouldCallBaseClass()
	{
		var mock = Mock.Create<MyBaseClassWithVirtualCalls>(MockBehavior.Default with { BaseClassBehavior = BaseClassBehavior.OnlyCallBaseClass });

		mock.Subject.VirtualVoidMethod(2);
		mock.Subject.VirtualVoidMethod(4);
		var result = mock.Subject.Sum;

		await That(result).IsEqualTo(6);
		await That(mock.Subject.VirtualMethodCallCount).IsEqualTo(0);
	}

	public class MyBaseClassWithVirtualCalls
	{
		private int _sum;
		public int Sum => _sum;

		public int VirtualMethodCallCount { get; private set; }
		public int VirtualMethodWithRefAndOutParametersCallCount { get; private set; }

		public virtual int VirtualProperty
		{
			get
			{
				return VirtualMethod()[0];
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
