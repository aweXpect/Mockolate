#if NET10_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;

namespace Mockolate.ExampleTests.Generated
{
	internal class MockForIChocolateDispenser : IChocolateDispenser
	{
		public void DispenseChocolate(string flavor)
			=> throw new NotImplementedException();

		public static bool IsActive { get; }
	}
}

namespace Mockolate.ExampleTests
{
	internal static partial class MockForIChocolateDispenserExtensions
	{
		extension(IChocolateDispenser _)
		{
			[OverloadResolutionPriority(10)]
			public static IChocolateDispenser CreateMock2()
			{
				return new Generated.MockForIChocolateDispenser();
			}
		}
	}
}
#endif
