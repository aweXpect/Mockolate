namespace Mockolate.Tests.MockMethods;

public sealed partial class InteractionsTests
{
	public sealed class ThrowingBaseTests
	{
		[Fact]
		public async Task VirtualMethod_WhenBaseThrows_ShouldStillRecordInvocation()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();

			void Act() => sut.DoThing(42);

			await That(Act).Throws<InvalidOperationException>();
			await That(sut.Mock.Verify.DoThing(It.IsAny<int>())).Once();
		}

		[Fact]
		public async Task VirtualMethod_WhenBaseThrows_ShouldRecordArgumentsPassedByCaller()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();

			try
			{
				sut.DoThing(7);
			}
			catch (InvalidOperationException)
			{
				// expected
			}

			await That(sut.Mock.Verify.DoThing(It.Is(7))).Once();
		}

		[Fact]
		public async Task VirtualVoidMethod_WhenBaseThrows_ShouldStillRecordInvocation()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();

			void Act() => sut.DoVoidThing();

			await That(Act).Throws<InvalidOperationException>();
			await That(sut.Mock.Verify.DoVoidThing()).Once();
		}

		public class ThrowingBaseService
		{
			public virtual int DoThing(int value) => throw new InvalidOperationException("base throws");

			public virtual void DoVoidThing() => throw new InvalidOperationException("base throws");
		}
	}
}
