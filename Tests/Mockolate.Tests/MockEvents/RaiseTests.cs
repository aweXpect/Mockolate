using Mockolate.Events;
using Mockolate.Exceptions;

namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	[Fact]
	public async Task AddEvent_WithoutMethod_ShouldThrowMockException()
	{
		Mock<IRaiseEvent> mock = Mock.Create<IRaiseEvent>();
		IMockRaises raises = mock.Raise;

		void Act()
			=> raises.AddEvent(nameof(IRaiseEvent.SomeEvent), this, null);

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event subscription may not be null.");
	}

	[Fact]
	public async Task RemoveEvent_WithoutMethod_ShouldThrowMockException()
	{
		Mock<IRaiseEvent> mock = Mock.Create<IRaiseEvent>();
		IMockRaises raises = mock.Raise;

		void Act()
			=> raises.RemoveEvent(nameof(IRaiseEvent.SomeEvent), this, null);

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event unsubscription may not be null.");
	}

	[Fact]
	public async Task Subscription_ShouldBeRegistered()
	{
		Mock<IRaiseEvent> mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (s, e) => { };

		mock.Subject.SomeEvent += handler;
		mock.Subject.SomeEvent += handler;

		await That(mock.Verify.SubscribedTo.SomeEvent()).Exactly(2);
		await That(mock.Verify.UnsubscribedFrom.SomeEvent()).Never();
	}

	[Fact]
	public async Task Unsubscription_ShouldBeRegistered()
	{
		Mock<IRaiseEvent> mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (s, e) => { };

		mock.Subject.SomeEvent -= handler;

		await That(mock.Verify.SubscribedTo.SomeEvent()).Never();
		await That(mock.Verify.UnsubscribedFrom.SomeEvent()).Once();
	}

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

	[Fact]
	public async Task WhenUsingRaise_ShouldInvokeEvent()
	{
		int callCount = 0;
		Mock<IRaiseEvent> mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (s, e) => { callCount++; };

		mock.Subject.SomeEvent += handler;
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Subject.SomeEvent -= handler;
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Raise.SomeEvent(this, EventArgs.Empty);

		await That(callCount).IsEqualTo(2);
	}

	public interface IMyEventService : IMyEventServiceBase1
	{
		new event EventHandler<string> SomeEvent;
	}

	public interface IMyEventServiceBase1
	{
		event EventHandler<long> SomeEvent;
	}

	public interface IRaiseEvent
	{
		event EventHandler? SomeEvent;
	}
}
