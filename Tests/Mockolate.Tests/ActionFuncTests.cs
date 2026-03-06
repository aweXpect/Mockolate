using System.Collections.Generic;

namespace Mockolate.Tests;

public class ActionFuncTests
{
	[Fact]
	public async Task MethodWithTooManyParameters_ShouldWorkWithSetupCallbacks()
	{
		List<int> received = [];
		IMyService mock = Mock.Create<IMyService>();
		mock.SetupMock.Method.MethodWithTooManyParameters(Match.AnyParameters())
			.Do((a, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) => { received.Add(a); })
			.Returns((a, b, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) => a + b);

		int result = mock.MethodWithTooManyParameters(42, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(result).IsEqualTo(44);
		await That(received).HasSingle().Which.IsEqualTo(42);
	}

	public interface IMyService
	{
		int MethodWithTooManyParameters(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17, int p18);
	}
}
