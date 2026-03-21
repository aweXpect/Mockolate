namespace Mockolate.Tests.MockEvents;

public sealed partial class RaiseTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task Subscription_ShouldBeRegistered()
		{
			MyRaiseEvent sut = MyRaiseEvent.CreateMock();
			EventHandler handler = (_, _) => { };

			sut.SubscribeToSomeEvent += handler;
			sut.SubscribeToSomeEvent += handler;

			await That(sut.Mock.Verify.SomeEvent.Subscribed()).Exactly(2);
			await That(sut.Mock.Verify.SomeEvent.Unsubscribed()).Never();
		}

		[Fact]
		public async Task Unsubscription_ShouldBeRegistered()
		{
			MyRaiseEvent sut = MyRaiseEvent.CreateMock();
			EventHandler handler = (_, _) => { };

			sut.SubscribeToSomeEvent -= handler;

			await That(sut.Mock.Verify.SomeEvent.Subscribed()).Never();
			await That(sut.Mock.Verify.SomeEvent.Unsubscribed()).Once();
		}

		[Fact]
		public async Task WhenUsingRaise_AnyParameters_ShouldInvokeEvent()
		{
			int callCount = 0;
			MyRaiseEvent sut = MyRaiseEvent.CreateMock();
			EventHandler handler = (_, _) => { callCount++; };

			sut.SubscribeToSomeEvent += handler;
			sut.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
			sut.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
			sut.SubscribeToSomeEvent -= handler;
			sut.Mock.Raise.SomeEvent(Match.WithDefaultParameters());
			sut.Mock.Raise.SomeEvent(Match.WithDefaultParameters());

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task WhenUsingRaise_ShouldInvokeEvent()
		{
			int callCount = 0;
			MyRaiseEvent sut = MyRaiseEvent.CreateMock();
			EventHandler handler = (_, _) => { callCount++; };

			sut.SubscribeToSomeEvent += handler;
			sut.Mock.Raise.SomeEvent(this, EventArgs.Empty);
			sut.Mock.Raise.SomeEvent(this, EventArgs.Empty);
			sut.SubscribeToSomeEvent -= handler;
			sut.Mock.Raise.SomeEvent(this, EventArgs.Empty);
			sut.Mock.Raise.SomeEvent(this, EventArgs.Empty);

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
