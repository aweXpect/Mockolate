using System;
using System.Runtime.CompilerServices;

namespace Mockolate.ExampleTests
{
	internal static partial class MockExtensions
	{
		extension<T>(T _)
		{
			public static T CreateMock2()
			{
				throw new NotSupportedException("This method is only meant to be used in the context of Mockolate and should not be called directly.");
			}
		}
	}
}
