using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.Setup;

public sealed class PropertySetupTests
{
	[Fact]
	public async Task AutoInitializeWith_WhenAlreadyInitialized_ShouldNotOverwriteValue()
	{
		FakePropertySetup setup = new("p");
		IInteractivePropertySetup interactive = setup;

		interactive.InitializeWith(5);
		interactive.InitializeWith(10);

		int value = interactive.InvokeGetter(null, MockBehavior.Default, () => -1);
		await That(value).IsEqualTo(5);
	}

	[Fact]
	public async Task DefaultInvokeGetter_WhenRequestedTypeDiffersFromBackingType_ShouldFallBackToGenerator()
	{
		// 0x40400000 reinterpreted via Unsafe.As<int, float> would yield 3.0f; the correct path
		// must take the typeof-equality branch and fall through to the defaultValueGenerator.
		PropertySetup.Default<int> setup = new("p", 0x40400000);
		IInteractivePropertySetup interactive = setup;

		float value = interactive.InvokeGetter(null, MockBehavior.Default, () => 99f);

		await That(value).IsEqualTo(99f);
	}

	[Fact]
	public async Task UserInitializeWith_SecondCall_ShouldNotOverwriteValue()
	{
		FakePropertySetup setup = new("p");
		IPropertySetup<int> userFacing = setup;

		userFacing.InitializeWith(5);
		userFacing.InitializeWith(10);

		int value = ((IInteractivePropertySetup)setup).InvokeGetter(null, MockBehavior.Default, () => -1);
		await That(value).IsEqualTo(5);
	}
}
