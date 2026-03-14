using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class InitializeTests
	{
		[Fact]
		public async Task Initialize_DirectSetupsTakePrecedence()
		{
			MockBehavior behavior = MockBehavior.Default.Initialize<IChocolateDispenser>(setup
				=> setup[It.Satisfies<string>(s => s.StartsWith("da"))].InitializeWith(5));

			IChocolateDispenser mock = IChocolateDispenser.CreateMock(behavior,
				setup => setup[It.Satisfies<string>(s => s.EndsWith("rk"))].InitializeWith(16));

			int bothMatchResult = mock["dark"];
			int directMatchResult = mock["--rk"];
			int behaviorMatchResult = mock["da--"];
			int noneMatchResult = mock["foo"];

			await That(bothMatchResult).IsEqualTo(16);
			await That(directMatchResult).IsEqualTo(16);
			await That(behaviorMatchResult).IsEqualTo(5);
			await That(noneMatchResult).IsEqualTo(0);
		}

		[Fact]
		public async Task Initialize_OtherType_ShouldIgnoreInitializations()
		{
			MockBehavior behavior =
				MockBehavior.Default.Initialize<IChocolateDispenser>(setup
					=> setup[It.Is("Dark")].InitializeWith(15));

			void Act()
			{
				_ = IMyService.CreateMock(behavior);
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task Initialize_ShouldApplySetupToCreatedMock()
		{
			MockBehavior behavior =
				MockBehavior.Default.Initialize<IChocolateDispenser>(setup
					=> setup[It.Is("Dark")].InitializeWith(15));

			IChocolateDispenser mock = IChocolateDispenser.CreateMock(behavior);

			int setupResult = mock["Dark"];
			int otherResult = mock["Light"];

			await That(setupResult).IsEqualTo(15);
			await That(otherResult).IsEqualTo(0);
		}
	}
}
