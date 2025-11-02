using Mockolate;

namespace Mockolate.Tests.Setup;

public sealed class MethodSetupsTests
{
	[Fact]
	public async Task ReturnMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.ReturnMethod17(
				1, 2, 3, 4, 5, 6, 7, 8,
				9, 10, 11, 12, 13, 14, 15, 16,
				17)
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> isCalled++)
			.Returns((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17);

		int result = mock.Subject.ReturnMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(153);
	}

	[Fact]
	public async Task ReturnMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.ReturnMethod18(
				1, 2, 3, 4, 5, 6, 7, 8,
				9, 10, 11, 12, 13, 14, 15, 16,
				17, 18)
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> isCalled++)
			.Returns((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17 + p18);

		int result = mock.Subject.ReturnMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(171);
	}

	[Fact]
	public async Task VoidMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.VoidMethod17(
				1, 2, 3, 4, 5, 6, 7, 8,
				9, 10, 11, 12, 13, 14, 15, 16,
				17)
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> isCalled++);

		mock.Subject.VoidMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task VoidMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.VoidMethod18(
				1, 2, 3, 4, 5, 6, 7, 8,
				9, 10, 11, 12, 13, 14, 15, 16,
				17, 18)
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> isCalled++);

		mock.Subject.VoidMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

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
