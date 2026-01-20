namespace Mockolate.ReproTests;

public sealed class StepsToReproduce
{
	[Fact]
	public async Task ReturnMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.ReturnMethod17(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++)
			.Returns((p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17);

		int result = mock.ReturnMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(153);
	}

	[Fact]
	public async Task ReturnMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.ReturnMethod18(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17), It.Is(18))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++)
			.Returns((p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17 + p18);

		int result = mock.ReturnMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(171);
	}

	[Fact]
	public async Task VoidMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.VoidMethod17(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++);

		mock.VoidMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task VoidMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.VoidMethod18(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17), It.Is(18))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++);

		mock.VoidMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
	}

	public interface IMyServiceWithMethodsWithMoreThan16Parameters
	{
		int ReturnMethod17(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17);

		int ReturnMethod18(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17, int p18);

		void VoidMethod17(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17);

		void VoidMethod18(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17, int p18);
	}
}
