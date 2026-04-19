namespace Mockolate.Tests.MockEvents;

public sealed class InteractionsThrowingCallbackTests
{
	[Fact]
	public async Task EventSubscribe_WhenSetupCallbackThrows_ShouldStillRecordSubscription()
	{
		ThrowingCallbackEventService sut = ThrowingCallbackEventService.CreateMock();
		sut.Mock.Setup.SomeEvent.OnSubscribed
			.Do(() => throw new InvalidOperationException("callback throws"));

		void Act() => sut.SomeEvent += Handler;

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify.SomeEvent.Subscribed()).Once();

		static void Handler(int value)
		{
		}
	}

	[Fact]
	public async Task EventUnsubscribe_WhenSetupCallbackThrows_ShouldStillRecordUnsubscription()
	{
		ThrowingCallbackEventService sut = ThrowingCallbackEventService.CreateMock();
		sut.Mock.Setup.SomeEvent.OnUnsubscribed
			.Do(() => throw new InvalidOperationException("callback throws"));

		void Act() => sut.SomeEvent -= Handler;

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify.SomeEvent.Unsubscribed()).Once();

		static void Handler(int value)
		{
		}
	}

	public delegate void ThrowingCallbackEventHandler(int value);

	public class ThrowingCallbackEventService
	{
		public virtual event ThrowingCallbackEventHandler? SomeEvent;

		public void Raise(int value) => SomeEvent?.Invoke(value);
	}
}
