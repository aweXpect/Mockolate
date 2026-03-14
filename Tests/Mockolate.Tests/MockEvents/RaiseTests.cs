using Mockolate.Exceptions;

namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	[Fact]
	public async Task AddEvent_WithoutMethod_ShouldThrowMockException()
	{
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		MockRegistration registration = ((IMock)mock).Registrations;

		void Act()
		{
			registration.AddEvent(nameof(IRaiseEvent.SomeEvent), this, null);
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event subscription may not be null.");
	}

	[Fact]
	public async Task RemoveEvent_WithoutMethod_ShouldThrowMockException()
	{
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		MockRegistration registration = ((IMock)mock).Registrations;

		void Act()
		{
			registration.RemoveEvent(nameof(IRaiseEvent.SomeEvent), this, null);
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event unsubscription may not be null.");
	}

	[Fact]
	public async Task Subscription_ShouldBeRegistered()
	{
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { };

		mock.SomeEvent += handler;
		mock.SomeEvent += handler;

		await That(mock.Mock.Verify.SomeEvent.Subscribed()).Exactly(2);
		await That(mock.Mock.Verify.SomeEvent.Unsubscribed()).Never();
	}

	[Fact]
	public async Task Unsubscription_ShouldBeRegistered()
	{
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { };

		mock.SomeEvent -= handler;

		await That(mock.Mock.Verify.SomeEvent.Subscribed()).Never();
		await That(mock.Mock.Verify.SomeEvent.Unsubscribed()).Once();
	}

	[Fact]
	public async Task WhenMockInheritsEventMultipleTimes()
	{
		IMyEventService mock = IMyEventService.CreateMock().Implementing<IMyEventServiceBase1>();
		int callCount = 0;

		mock.SomeEvent += Subject_SomeEvent;
		mock.Mock.Raise.SomeEvent(this, "event data");
		mock.SomeEvent -= Subject_SomeEvent;

		await That(mock.Mock.Verify.SomeEvent.Subscribed()).Once();
		await That(mock.Mock.Verify.SomeEvent.Unsubscribed()).Once();
		await That(callCount).IsEqualTo(1);

		void Subject_SomeEvent(object? sender, string e)
		{
			callCount++;
		}
	}

	[Fact]
	public async Task WhenSubscribedToOtherEvent_ShouldNotTrigger()
	{
		int callCount = 0;
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeOtherEvent += handler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.SomeOtherEvent -= handler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WhenSubscriptionThrows_ShouldNotWrapException()
	{
		IMyEventService mock = IMyEventService.CreateMock().Implementing<IMyEventServiceBase1>();

		mock.SomeEvent += SubscriptionThrowingException;

		void Act()
		{
			mock.Mock.Raise.SomeEvent(this, "event data");
		}

		await That(Act).Throws<MockException>().WithMessage("Subscription exception");

		void SubscriptionThrowingException(object? sender, string e)
		{
			throw new MockException("Subscription exception");
		}
	}

	[Fact]
	public async Task WhenUnsubscribedFromOtherEvent_ShouldNotAffectOtherSubscriptions()
	{
		int callCount = 0;
		int otherCallCount = 0;
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { callCount++; };
		EventHandler otherHandler = (_, _) => { otherCallCount++; };

		mock.SomeEvent += handler;
		mock.SomeOtherEvent += otherHandler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeOtherEvent(Match.WithDefaultParameters());
		mock.SomeOtherEvent -= otherHandler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeOtherEvent(Match.WithDefaultParameters());
		mock.SomeEvent -= handler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeOtherEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(2);
		await That(otherCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WhenUsingRaise_AnyParameters_ShouldInvokeEvent()
	{
		int callCount = 0;
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeEvent += handler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.SomeEvent -= handler;
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
		mock.Mock.Raise.SomeEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(2);
	}

	[Fact]
	public async Task WhenUsingRaise_ShouldInvokeEvent()
	{
		int callCount = 0;
		IRaiseEvent mock = IRaiseEvent.CreateMock();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeEvent += handler;
		mock.Mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.SomeEvent -= handler;
		mock.Mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Mock.Raise.SomeEvent(this, EventArgs.Empty);

		await That(callCount).IsEqualTo(2);
	}

	[Fact]
	public async Task WhenUsingRaise_WithoutRegistration_ShouldNotThrow()
	{
		IRaiseEvent mock = IRaiseEvent.CreateMock();

		void Act()
		{
			mock.Mock.Raise.SomeEvent(this, EventArgs.Empty);
		}

		await That(Act).DoesNotThrow();
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
		event EventHandler? SomeOtherEvent;
	}
}
