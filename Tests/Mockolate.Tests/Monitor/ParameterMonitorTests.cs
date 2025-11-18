using Mockolate.Monitor;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Monitor;

public sealed class ParameterMonitorTests
{
	[Fact]
	public async Task Monitor_ShouldMonitorReceivedValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(Any<string>().Monitor(out ParameterMonitor<string> monitor), Any<int>());

		mock.Dispense("Dark", 5);
		mock.Dispense("White", 5);

		await That(monitor.Values).IsEqualTo(["Dark", "White",]);
	}

	[Fact]
	public async Task Monitor_WithFilter_ShouldMonitorMatchingValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(Any<string>(), With<int>(x => x > 4).Monitor(out ParameterMonitor<int> monitor));

		mock.Dispense("Dark", 3);
		mock.Dispense("White", 5);
		mock.Dispense("White", 4);
		mock.Dispense("White", 6);
		mock.Dispense("White", 7);
		mock.Dispense("White", 1);
		mock.Dispense("White", 8);

		await That(monitor.Values).IsEqualTo([5, 6, 7, 8,]);
	}

	[Fact]
	public async Task MultipleMonitors_ShouldAllMonitorValues()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.SetupMock.Method.Dispense(
			Any<string>()
				.Monitor(out ParameterMonitor<string> monitorA),
			Any<int>()
				.Monitor(out ParameterMonitor<int> monitorB1)
				.Monitor(out ParameterMonitor<int> monitorB2));

		mock.Dispense("Dark", 5);
		mock.Dispense("White", 6);

		await That(monitorA.Values).IsEqualTo(["Dark", "White",]);
		await That(monitorB1.Values).IsEqualTo([5, 6,]);
		await That(monitorB2.Values).IsEqualTo([5, 6,]);
	}
}
