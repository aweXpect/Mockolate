using aweXpect.Chronology;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public sealed class VerificationResultMutationTests
{
	[Test]
	public async Task AwaitableVerify_AfterMatching_ShouldMarkMatchingInteractionsAsVerified()
	{
		IMyService sut = IMyService.CreateMock();
		sut.Mock.Setup.SomeFlag.InitializeWith(true);
		_ = sut.SomeFlag;

		sut.Mock.Verify.SomeFlag.Got().Within(10.Milliseconds()).Exactly(1);

		IMockInteractions interactions = ((IMock)sut).MockRegistry.Interactions;
		await That(interactions.GetUnverifiedInteractions()).IsEmpty();
	}

	[Test]
	public async Task AwaitableVerify_WithRecordingDisabled_ShouldThrowMockException()
	{
		IMyService sut = IMyService.CreateMock(MockBehavior.Default.SkippingInteractionRecording());
		sut.Mock.Setup.SomeFlag.InitializeWith(true);
		_ = sut.SomeFlag;

		void Act()
		{
			sut.Mock.Verify.SomeFlag.Got().Within(10.Milliseconds()).Exactly(1);
		}

		await That(Act).Throws<MockException>()
			.WithMessage("*interaction recording is disabled*").AsWildcard();
	}
}
