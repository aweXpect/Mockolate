using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MatchExtensionsTests
{
	[Test]
	public async Task OutParameterMonitor_MultipleMonitors_ShouldAllMonitorValues()
	{
		int idx = 1;
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MyMethodWithOutParam(It.IsOut(() => idx++)
			.Monitor(out IParameterMonitor<int> monitor1)
			.Monitor(out IParameterMonitor<int> monitor2));

		mock.MyMethodWithOutParam(out _);
		mock.MyMethodWithOutParam(out _);
		mock.MyMethodWithOutParam(out _);

		await That(monitor1.Values).IsEqualTo([1, 2, 3,]);
		await That(monitor2.Values).IsEqualTo([1, 2, 3,]);
	}

	[Test]
	public async Task OutParameterMonitor_ShouldMonitorReceivedValues()
	{
		int idx = 1;
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MyMethodWithOutParam(
			It.IsOut(() => idx++).Monitor(out IParameterMonitor<int> monitor));

		mock.MyMethodWithOutParam(out _);
		mock.MyMethodWithOutParam(out _);

		await That(monitor.Values).IsEqualTo([1, 2,]);
	}

	[Test]
	public async Task ParameterMonitor_MultipleMonitors_ShouldAllMonitorValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(
			It.IsAny<string>()
				.Monitor(out IParameterMonitor<string> monitorA),
			It.IsAny<int>()
				.Monitor(out IParameterMonitor<int> monitorB1)
				.Monitor(out IParameterMonitor<int> monitorB2));

		mock.Dispense("Dark", 5);
		mock.Dispense("White", 6);

		await That(monitorA.Values).IsEqualTo(["Dark", "White",]);
		await That(monitorB1.Values).IsEqualTo([5, 6,]);
		await That(monitorB2.Values).IsEqualTo([5, 6,]);
	}

	[Test]
	public async Task ParameterMonitor_ShouldMonitorReceivedValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(It.IsAny<string>().Monitor(out IParameterMonitor<string> monitor), It.IsAny<int>());

		mock.Dispense("Dark", 5);
		mock.Dispense("White", 5);

		await That(monitor.Values).IsEqualTo(["Dark", "White",]);
	}

	[Test]
	public async Task ParameterMonitor_WithFilter_ShouldMonitorMatchingValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(It.IsAny<string>(),
			It.Satisfies<int>(x => x > 4).Monitor(out IParameterMonitor<int> monitor));

		mock.Dispense("Dark", 3);
		mock.Dispense("White", 5);
		mock.Dispense("White", 4);
		mock.Dispense("White", 6);
		mock.Dispense("White", 7);
		mock.Dispense("White", 1);
		mock.Dispense("White", 8);

		await That(monitor.Values).IsEqualTo([5, 6, 7, 8,]);
	}

	[Test]
	public async Task RefParameterMonitor_MultipleMonitors_ShouldAllMonitorValues()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MyMethodWithRefParam(It.IsRef<int>(v => 2 * v)
			.Monitor(out IParameterMonitor<int> monitor1)
			.Monitor(out IParameterMonitor<int> monitor2));
		int value1 = 3;
		int value2 = 7;

		mock.MyMethodWithRefParam(ref value1);
		mock.MyMethodWithRefParam(ref value2);

		await That(monitor1.Values).IsEqualTo([6, 14,]);
		await That(monitor2.Values).IsEqualTo([6, 14,]);
	}

	[Test]
	public async Task RefParameterMonitor_ShouldMonitorReceivedValues()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MyMethodWithRefParam(It.IsRef<int>(v => 2 * v)
			.Monitor(out IParameterMonitor<int> monitor));
		int value1 = 3;
		int value2 = 7;

		mock.MyMethodWithRefParam(ref value1);
		mock.MyMethodWithRefParam(ref value2);

		await That(monitor.Values).IsEqualTo([6, 14,]);
	}

	[Test]
	public async Task RefParameterMonitor_WithFilter_ShouldMonitorMatchingValues()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MyMethodWithRefParam(It.IsRef<int>(i => i > 4, v => 2 * v)
			.Monitor(out IParameterMonitor<int> monitor));

		int[] values = [3, 5, 4, 6, 7, 1, 8,];
		foreach (int value in values)
		{
			int refValue = value;
			mock.MyMethodWithRefParam(ref refValue);
		}

		await That(monitor.Values).IsEqualTo([10, 12, 14, 16,]);
	}
}
