using Mockolate.Monitor;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockExtensionsTests
{
	public sealed class ClearAllInteractionsTests
	{
		[Fact]
		public async Task Monitor_ShouldWorkAcrossClearAllInteractions()
		{
			IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

			mock.Dispense("Dark", 1);
			MockMonitor<IChocolateDispenser> monitor;
			using (_ = mock.MonitorMock(out monitor))
			{
				mock.Dispense("Light", 2);
				mock.SetupMock.ClearAllInteractions();
				mock.Dispense("Dark", 3);
			}

			mock.Dispense("Light", 4);

			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(1))).Never();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Light"), It.Is(2))).Never();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(3))).Once();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Light"), It.Is(4))).Once();

			await That(monitor.Verify.Invoked.Dispense(It.Is("Dark"), It.Is(1))).Never();
			await That(monitor.Verify.Invoked.Dispense(It.Is("Light"), It.Is(2))).Once();
			await That(monitor.Verify.Invoked.Dispense(It.Is("Dark"), It.Is(3))).Once();
			await That(monitor.Verify.Invoked.Dispense(It.Is("Light"), It.Is(4))).Never();
		}

		[Fact]
		public async Task ShouldClearRecordedInteractions()
		{
			IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

			mock.Dispense("Dark", 1);
			mock.Dispense("Light", 2);
			mock.SetupMock.ClearAllInteractions();
			mock.Dispense("Dark", 3);
			mock.Dispense("Light", 4);

			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(1))).Never();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Light"), It.Is(2))).Never();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(3))).Once();
			await That(mock.VerifyMock.Invoked.Dispense(It.Is("Light"), It.Is(4))).Once();
		}
	}
}
