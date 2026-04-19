namespace Mockolate.Tests.MockIndexers;

public sealed class InteractionsThrowingBaseTests
{
	[Fact]
	public async Task VirtualIndexerGetter_WhenBaseThrows_ShouldStillRecordAccess()
	{
		ThrowingBaseIndexer sut = ThrowingBaseIndexer.CreateMock();

		void Act() => _ = sut[3];

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify[It.Is(3)].Got()).Once();
	}

	[Fact]
	public async Task VirtualIndexerSetter_WhenBaseThrows_ShouldStillRecordAccess()
	{
		ThrowingBaseIndexer sut = ThrowingBaseIndexer.CreateMock();

		void Act() => sut[3] = "value";

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify[It.Is(3)].Set(It.Is("value"))).Once();
	}

	public class ThrowingBaseIndexer
	{
		public virtual string this[int key]
		{
			get => throw new InvalidOperationException("base getter throws");
			set => throw new InvalidOperationException("base setter throws");
		}
	}
}
