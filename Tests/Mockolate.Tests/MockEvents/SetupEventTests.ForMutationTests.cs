using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockEvents;

public sealed class SetupEventForMutationTests
{
	[Fact]
	public async Task OnSubscribed_For2_HoldsFirstCallbackForTwoSubscriptionsBeforeAdvancing()
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
		sut.ChocolateDispensed += Handler;

		await That(count1).IsEqualTo(3);
		await That(count2).IsEqualTo(1);

		void Handler(string type, int amount) { }
	}

	[Fact]
	public async Task OnUnsubscribed_For2_HoldsFirstCallbackForTwoUnsubscriptionsBeforeAdvancing()
	{
		int count1 = 0;
		int count2 = 0;
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Mock.Setup.ChocolateDispensed
			.OnUnsubscribed.Do(() => { count1++; }).For(2)
			.OnUnsubscribed.Do(() => { count2++; });

		sut.ChocolateDispensed -= Handler;
		sut.ChocolateDispensed -= Handler;
		sut.ChocolateDispensed -= Handler;
		sut.ChocolateDispensed -= Handler;

		await That(count1).IsEqualTo(3);
		await That(count2).IsEqualTo(1);

		void Handler(string type, int amount) { }
	}
}
