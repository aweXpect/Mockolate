using Mockolate.Exceptions;

namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	[Fact]
	public async Task AddEvent_WithoutMethod_ShouldThrowMockException()
	{
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

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
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

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
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { };

		mock.SomeEvent += handler;
		mock.SomeEvent += handler;

		await That(mock.VerifyMock.SubscribedTo.SomeEvent()).Exactly(2);
		await That(mock.VerifyMock.UnsubscribedFrom.SomeEvent()).Never();
	}

	[Fact]
	public async Task Unsubscription_ShouldBeRegistered()
	{
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { };

		mock.SomeEvent -= handler;

		await That(mock.VerifyMock.SubscribedTo.SomeEvent()).Never();
		await That(mock.VerifyMock.UnsubscribedFrom.SomeEvent()).Once();
	}

	[Fact]
	public async Task WhenMockInheritsEventMultipleTimes()
	{
		IMyEventService mock = Mock.Create<IMyEventService, IMyEventServiceBase1>();
		int callCount = 0;

		mock.SomeEvent += Subject_SomeEvent;
		mock.RaiseOnMock.SomeEvent(this, "event data");
		mock.SomeEvent -= Subject_SomeEvent;

		await That(mock.VerifyMock.SubscribedTo.SomeEvent()).Once();
		await That(mock.VerifyMock.UnsubscribedFrom.SomeEvent()).Once();
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
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeOtherEvent += handler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.SomeOtherEvent -= handler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task WhenSubscriptionThrows_ShouldNotWrapException()
	{
		IMyEventService mock = Mock.Create<IMyEventService, IMyEventServiceBase1>();

		mock.SomeEvent += SubscriptionThrowingException;

		void Act()
		{
			mock.RaiseOnMock.SomeEvent(this, "event data");
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
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { callCount++; };
		EventHandler otherHandler = (_, _) => { otherCallCount++; };

		mock.SomeEvent += handler;
		mock.SomeOtherEvent += otherHandler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeOtherEvent(Match.WithDefaultParameters());
		mock.SomeOtherEvent -= otherHandler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeOtherEvent(Match.WithDefaultParameters());
		mock.SomeEvent -= handler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeOtherEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(2);
		await That(otherCallCount).IsEqualTo(1);
	}

	[Fact]
	public async Task WhenUsingRaise_AnyParameters_ShouldInvokeEvent()
	{
		int callCount = 0;
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeEvent += handler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.SomeEvent -= handler;
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());
		mock.RaiseOnMock.SomeEvent(Match.WithDefaultParameters());

		await That(callCount).IsEqualTo(2);
	}

	[Fact]
	public async Task WhenUsingRaise_ShouldInvokeEvent()
	{
		int callCount = 0;
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();
		EventHandler handler = (_, _) => { callCount++; };

		mock.SomeEvent += handler;
		mock.RaiseOnMock.SomeEvent(this, EventArgs.Empty);
		mock.RaiseOnMock.SomeEvent(this, EventArgs.Empty);
		mock.SomeEvent -= handler;
		mock.RaiseOnMock.SomeEvent(this, EventArgs.Empty);
		mock.RaiseOnMock.SomeEvent(this, EventArgs.Empty);

		await That(callCount).IsEqualTo(2);
	}

	[Fact]
	public async Task WhenUsingRaise_WithoutRegistration_ShouldNotThrow()
	{
		IRaiseEvent mock = Mock.Create<IRaiseEvent>();

		void Act()
		{
			mock.RaiseOnMock.SomeEvent(this, EventArgs.Empty);
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
