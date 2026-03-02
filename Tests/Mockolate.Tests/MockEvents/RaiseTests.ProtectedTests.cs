namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task Subscription_ShouldBeRegistered()
		{
			MyRaiseEvent mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (_, _) => { };

			mock.SubscribeToSomeEvent += handler;
			mock.SubscribeToSomeEvent += handler;

			await That(mock.VerifyMock.SubscribedTo.Protected.SomeEvent()).Exactly(2);
			await That(mock.VerifyMock.UnsubscribedFrom.Protected.SomeEvent()).Never();
		}

		[Fact]
		public async Task Unsubscription_ShouldBeRegistered()
		{
			MyRaiseEvent mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (_, _) => { };

			mock.SubscribeToSomeEvent -= handler;

			await That(mock.VerifyMock.SubscribedTo.Protected.SomeEvent()).Never();
			await That(mock.VerifyMock.UnsubscribedFrom.Protected.SomeEvent()).Once();
		}

		[Fact]
		public async Task WhenUsingRaise_AnyParameters_ShouldInvokeEvent()
		{
			int callCount = 0;
			MyRaiseEvent mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (_, _) => { callCount++; };

			mock.SubscribeToSomeEvent += handler;
			mock.RaiseOnMock.Protected.SomeEvent(Match.WithDefaultParameters());
			mock.RaiseOnMock.Protected.SomeEvent(Match.WithDefaultParameters());
			mock.SubscribeToSomeEvent -= handler;
			mock.RaiseOnMock.Protected.SomeEvent(Match.WithDefaultParameters());
			mock.RaiseOnMock.Protected.SomeEvent(Match.WithDefaultParameters());

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task WhenUsingRaise_ShouldInvokeEvent()
		{
			int callCount = 0;
			MyRaiseEvent mock = Mock.Create<MyRaiseEvent>();
			EventHandler handler = (_, _) => { callCount++; };

			mock.SubscribeToSomeEvent += handler;
			mock.RaiseOnMock.Protected.SomeEvent(this, EventArgs.Empty);
			mock.RaiseOnMock.Protected.SomeEvent(this, EventArgs.Empty);
			mock.SubscribeToSomeEvent -= handler;
			mock.RaiseOnMock.Protected.SomeEvent(this, EventArgs.Empty);
			mock.RaiseOnMock.Protected.SomeEvent(this, EventArgs.Empty);

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
