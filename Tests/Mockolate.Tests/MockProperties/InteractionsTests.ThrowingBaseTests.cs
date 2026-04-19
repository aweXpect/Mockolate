namespace Mockolate.Tests.MockProperties;

public sealed class InteractionsThrowingBaseTests
{
	[Fact]
	public async Task VirtualPropertyGetter_WhenBaseThrows_ShouldStillRecordAccess()
	{
		ThrowingBaseService sut = ThrowingBaseService.CreateMock();

		void Act() => _ = sut.Value;

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify.Value.Got()).Once();
	}

	[Fact]
	public async Task VirtualPropertySetter_WhenBaseThrows_ShouldStillRecordAccess()
	{
		ThrowingBaseService sut = ThrowingBaseService.CreateMock();

		void Act() => sut.Value = 42;

		await That(Act).Throws<InvalidOperationException>();
		await That(sut.Mock.Verify.Value.Set(It.Is(42))).Once();
	}

	public class ThrowingBaseService
	{
		public virtual int Value
		{
			get => throw new InvalidOperationException("base getter throws");
			set => throw new InvalidOperationException("base setter throws");
		}
	}
}
