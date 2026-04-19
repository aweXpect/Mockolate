using Mockolate.Parameters;

namespace Mockolate.Tests.MockMethods;

public sealed partial class InteractionsTests
{
	public sealed class ThrowingBaseTests
	{
		[Fact]
		public async Task VirtualMethod_WhenBaseThrows_ShouldRecordArgumentsPassedByCaller()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();
			sut.Mock.Setup.DoThing(It.IsAny<int>().Monitor(out IParameterMonitor<int> values)).Returns(1);

			void Act() => sut.DoThing(42);

			await That(Act).Throws<InvalidOperationException>();
			await That(values.Values).HasSingle().Which.IsEqualTo(42);
		}

		[Fact]
		public async Task VirtualMethod_WhenBaseThrows_ShouldStillRecordInvocation()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();

			void Act() => sut.DoThing(42);

			await That(Act).Throws<InvalidOperationException>();
			await That(sut.Mock.Verify.DoThing(42)).Once();
		}

		[Fact]
		public async Task VirtualVoidMethod_WhenBaseThrows_ShouldRecordArgumentsPassedByCaller()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();
			sut.Mock.Setup.DoVoidThing(It.IsAny<int>().Monitor(out IParameterMonitor<int> values)).DoesNotThrow();

			void Act() => sut.DoVoidThing(42);

			await That(Act).Throws<InvalidOperationException>();
			await That(values.Values).HasSingle().Which.IsEqualTo(42);
		}

		[Fact]
		public async Task VirtualVoidMethod_WhenBaseThrows_ShouldStillRecordInvocation()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();

			void Act() => sut.DoVoidThing(1);

			await That(Act).Throws<InvalidOperationException>();
			await That(sut.Mock.Verify.DoVoidThing(1)).Once();
		}

		public class ThrowingBaseService
		{
			public virtual int DoThing(int value) => throw new InvalidOperationException("base throws");

			public virtual void DoVoidThing(int value) => throw new InvalidOperationException("base throws");
		}
	}
}
