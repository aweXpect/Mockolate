using Mockolate.Events;
using Mockolate.Exceptions;

namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task AddEvent_WithoutMethod_ShouldThrowMockException()
		{
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			IMockRaises raises = mock.Raise.Protected;

			void Act()
				=> raises.AddEvent("SomeEvent", this, null);

			await That(Act).Throws<MockException>()
				.WithMessage("The method of an event subscription may not be null.");
		}

		[Fact]
		public async Task RemoveEvent_WithoutMethod_ShouldThrowMockException()
		{
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			IMockRaises raises = mock.Raise.Protected;

			void Act()
				=> raises.RemoveEvent("SomeEvent", this, null);

			await That(Act).Throws<MockException>()
				.WithMessage("The method of an event unsubscription may not be null.");
		}

		[Fact]
		public async Task Subscription_ShouldBeRegistered()
		{
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (s, e) => { };

			mock.Subject.SubscribeToSomeEvent += handler;
			mock.Subject.SubscribeToSomeEvent += handler;

			await That(mock.Verify.SubscribedTo.Protected.SomeEvent()).Exactly(2);
			await That(mock.Verify.UnsubscribedFrom.Protected.SomeEvent()).Never();
		}

		[Fact]
		public async Task Unsubscription_ShouldBeRegistered()
		{
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (s, e) => { };

			mock.Subject.SubscribeToSomeEvent -= handler;

			await That(mock.Verify.SubscribedTo.Protected.SomeEvent()).Never();
			await That(mock.Verify.UnsubscribedFrom.Protected.SomeEvent()).Once();
		}

		[Fact]
		public async Task WhenUsingRaise_ShouldInvokeEvent()
		{
			int callCount = 0;
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (s, e) => { callCount++; };

			mock.Subject.SubscribeToSomeEvent += handler;
			mock.Raise.Protected.SomeEvent(this, EventArgs.Empty);
			mock.Raise.Protected.SomeEvent(this, EventArgs.Empty);
			mock.Subject.SubscribeToSomeEvent -= handler;
			mock.Raise.Protected.SomeEvent(this, EventArgs.Empty);
			mock.Raise.Protected.SomeEvent(this, EventArgs.Empty);

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task WhenUsingRaise_WithAnyParameters_ShouldInvokeEvent()
		{
			int callCount = 0;
			Mock<MyRaiseEvent> mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (s, e) => { callCount++; };

			mock.Subject.SubscribeToSomeEvent += handler;
			mock.Raise.Protected.SomeEvent(WithDefaultParameters());
			mock.Raise.Protected.SomeEvent(WithDefaultParameters());
			mock.Subject.SubscribeToSomeEvent -= handler;
			mock.Raise.Protected.SomeEvent(WithDefaultParameters());
			mock.Raise.Protected.SomeEvent(WithDefaultParameters());

			await That(callCount).IsEqualTo(2);
		}

#pragma warning disable CS0067 // Event is never used
#pragma warning disable CA1070 // Do not declare event fields as virtual
		public class MyRaiseEvent
		{
			public event EventHandler? SubscribeToSomeEvent
			{
				add => SomeEvent += value;
				remove => SomeEvent -= value;
			}

			protected virtual event EventHandler? SomeEvent;
		}
#pragma warning restore CA1070 // Do not declare event fields as virtual
#pragma warning restore CS0067 // Event is never used
	}
}
