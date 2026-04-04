using System.Collections.Generic;
using System.Reflection;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockEvents;

public sealed class SetupEventTests
{
	[Fact]
	public async Task Forever_Extension_RepeatsIndefinitely()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed
			.OnSubscribed.Do(() => { callCount++; }).For(1).Forever();

		for (int i = 0; i < 10; i++)
		{
			sut.ChocolateDispensed += Handler;
		}

		await That(callCount).IsEqualTo(10);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task MultipleSetups_AreAllInvoked()
	{
		int callCount1 = 0;
		int callCount2 = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount1++; });
		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount2++; });

		sut.ChocolateDispensed += Handler;

		await That(callCount1).IsEqualTo(1);
		await That(callCount2).IsEqualTo(1);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnlyOnce_Extension_StopsAfterSingleInvocation()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount++; }).OnlyOnce();

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;

		await That(callCount).IsEqualTo(1);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_Do_Action_IsInvokedOnSubscribe()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount++; });

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;

		await That(callCount).IsEqualTo(2);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_Do_TargetMethod_ReceivesCorrectValues()
	{
		List<MethodInfo> received = [];
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do((_, method) => { received.Add(method); });

		sut.ChocolateDispensed += Handler;

		await That(received).HasCount().EqualTo(1);
		await That(received[0].Name).Contains("Handler");

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_DoesNotFireOnUnsubscribe()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount++; });

		void Handler(string type, int amount) { }
		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed -= Handler;

		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task OnSubscribed_For_RepeatsCallbackNTimes()
	{
		int count1 = 0;
		int count2 = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed
			.OnSubscribed.Do(() => { count1++; }).For(2)
			.OnSubscribed.Do(() => { count2++; });

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;

		await That(count1).IsEqualTo(2);
		await That(count2).IsEqualTo(1);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_InParallel_RunsAlongsideNextCallback()
	{
		int count1 = 0;
		int count2 = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed
			.OnSubscribed.Do(() => { count1++; }).InParallel()
			.OnSubscribed.Do(() => { count2++; });

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;

		await That(count1).IsEqualTo(2);
		await That(count2).IsEqualTo(2);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_Only_StopsAfterNInvocations()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount++; }).Only(2);

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed += Handler;

		await That(callCount).IsEqualTo(2);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnSubscribed_When_OnlyFires_WhenPredicateMatches()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { callCount++; }).When(n => n % 2 == 0);

		for (int i = 0; i < 6; i++)
		{
			sut.ChocolateDispensed += Handler;
		}

		// subscriptions at index 0, 2, 4 pass the predicate
		await That(callCount).IsEqualTo(3);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnUnsubscribed_Do_Action_IsInvokedOnUnsubscribe()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnUnsubscribed.Do(() => { callCount++; });

		void Handler(string type, int amount) { }

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed -= Handler;
		sut.ChocolateDispensed -= Handler;

		await That(callCount).IsEqualTo(2);
	}

	[Fact]
	public async Task OnUnsubscribed_Do_TargetMethod_ReceivesCorrectValues()
	{
		List<MethodInfo> received = [];
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnUnsubscribed.Do((_, method) => { received.Add(method); });

		void Handler(string type, int amount) { }

		sut.ChocolateDispensed += Handler;
		sut.ChocolateDispensed -= Handler;

		await That(received).HasCount().EqualTo(1);
		await That(received[0].Name).Contains("Handler");
	}

	[Fact]
	public async Task OnUnsubscribed_DoesNotFireOnSubscribe()
	{
		int callCount = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnUnsubscribed.Do(() => { callCount++; });

		void Handler(string type, int amount) { }
		sut.ChocolateDispensed += Handler;

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task SetupDoesNotInterfereWithVerification()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed.OnSubscribed.Do(() => { });

		void Handler(string type, int amount) { }
		sut.ChocolateDispensed += Handler;

		await That(sut.Mock.Verify.ChocolateDispensed.Subscribed()).Once();
	}
}
