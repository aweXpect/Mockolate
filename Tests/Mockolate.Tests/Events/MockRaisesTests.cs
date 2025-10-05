using System;
using System.Collections.Generic;
using System.Text;
using Mockolate.Events;
using Mockolate.Exceptions;

namespace Mockolate.Tests.Events;

public sealed class MockRaisesTests
{
	[Fact]
	public async Task AddEvent_WithoutMethod_ShouldThrowMockException()
	{
		var mock = Mock.For<IRaiseEvent>();
		IMockRaises raises = mock.Raise;

		void Act()
			=> raises.AddEvent(nameof(IRaiseEvent.SomeEvent), this, null);

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event subscription may not be null.");
	}

	[Fact]
	public async Task RemoveEvent_WithoutMethod_ShouldThrowMockException()
	{
		var mock = Mock.For<IRaiseEvent>();
		IMockRaises raises = mock.Raise;

		void Act()
			=> raises.RemoveEvent(nameof(IRaiseEvent.SomeEvent), this, null);

		await That(Act).Throws<MockException>()
			.WithMessage("The method of an event unsubscription may not be null.");
	}

	[Fact]
	public async Task Subscription_ShouldBeRegistered()
	{
		int callCount = 0;
		var mock = Mock.For<IRaiseEvent>();
		EventHandler handler = (s, e) => { callCount++; };

		mock.Object.SomeEvent += handler;
		mock.Object.SomeEvent += handler;

		await That(mock.Event.SomeEvent.Subscribed().Exactly(2));
		await That(mock.Event.SomeEvent.Unsubscribed().Never());
	}

	[Fact]
	public async Task Unsubscription_ShouldBeRegistered()
	{
		int callCount = 0;
		var mock = Mock.For<IRaiseEvent>();
		EventHandler handler = (s, e) => { callCount++; };

		mock.Object.SomeEvent -= handler;

		await That(mock.Event.SomeEvent.Subscribed().Never());
		await That(mock.Event.SomeEvent.Unsubscribed().Once());
	}

	[Fact]
	public async Task WhenUsingRaise_ShouldInvokeEvent()
	{
		int callCount = 0;
		var mock = Mock.For<IRaiseEvent>();
		EventHandler handler = (s, e) => { callCount++; };

		mock.Object.SomeEvent += handler;
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Object.SomeEvent -= handler;
		mock.Raise.SomeEvent(this, EventArgs.Empty);
		mock.Raise.SomeEvent(this, EventArgs.Empty);

		await That(callCount).IsEqualTo(2);
	}

	public interface IRaiseEvent
	{
		event EventHandler? SomeEvent;
	}
}
