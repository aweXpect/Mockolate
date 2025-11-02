namespace Mockolate.Tests.MockEvents;

public sealed partial class MockTests
{
	[Fact]
	public async Task WhenMockInheritsEventMultipleTimes()
	{
		Mock<IMyEventService, IMyEventServiceBase1> mock = Mock.Create<IMyEventService, IMyEventServiceBase1>();
		int callCount = 0;

		mock.Subject.SomeEvent += Subject_SomeEvent;
		mock.Raise.SomeEvent(this, "event data");
		mock.Subject.SomeEvent -= Subject_SomeEvent;

		await That(mock.Verify.SubscribedTo.SomeEvent()).Once();
		await That(mock.Verify.UnsubscribedFrom.SomeEvent()).Once();
		await That(callCount).IsEqualTo(1);

		void Subject_SomeEvent(object? sender, string e) => callCount++;
	}

	public interface IMyEventService : IMyEventServiceBase1
	{
		new event EventHandler<string> SomeEvent;
	}

	public interface IMyEventServiceBase1
	{
		event EventHandler<long> SomeEvent;
	}
}
