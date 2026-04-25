// Quick smoke test invoked via Program.cs to verify the suggested-fix mocks behave correctly
// before running the full BenchmarkDotNet harness.

using System;
using Mockolate.Benchmarks.RealisticSuggestedFix;
using Mockolate.Benchmarks.Suggested;

namespace Mockolate.Benchmarks;

internal static class SuggestedMocksSmokeTest
{
	public static void Run()
	{
		// Method
		SuggestedMy2ParamMock m = new();
		m.Mock.Setup.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Returns(true);
		for (int i = 0; i < 7; i++)
		{
			if (!m.MyFunc(42, "x"))
			{
				throw new InvalidOperationException("method returned false");
			}
		}

		m.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(7);

		// Property
		SuggestedCounterMock c = new();
		c.Mock.Setup.Counter.InitializeWith(42);
		for (int i = 0; i < 5; i++)
		{
			if (c.Counter != 42)
			{
				throw new InvalidOperationException("property wrong value");
			}
		}

		c.Mock.Verify.Counter.Got().Exactly(5);

		// Indexer
		SuggestedKeyIndexerMock idx = new();
		idx.Mock.Setup[It.IsAny<string>()].Returns(true);
		for (int i = 0; i < 3; i++)
		{
			if (!idx["key"])
			{
				throw new InvalidOperationException("indexer false");
			}
		}

		idx.Mock.Verify[It.IsAny<string>()].Got().Exactly(3);

		// Event
		SuggestedSomeEventMock e = new();
		for (int i = 0; i < 4; i++)
		{
			e.SomeEvent += static (_, _) => { };
		}

		e.Mock.Verify.SomeEvent.Subscribed().Exactly(4);

		// Realistic suggested (uses real MockRegistry / FastMockInteractions / setups)
		RealisticSuggestedMy2ParamMock real = new();
		real.Mock.Setup.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Returns(true);
		for (int i = 0; i < 6; i++)
		{
			if (!real.MyFunc(7, "y"))
			{
				throw new InvalidOperationException("realistic returned false");
			}
		}

		real.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(6);
	}
}
