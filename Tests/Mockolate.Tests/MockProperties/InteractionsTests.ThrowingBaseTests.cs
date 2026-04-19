namespace Mockolate.Tests.MockProperties;

public sealed partial class InteractionsTests
{
	public sealed class ThrowingBaseTests
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

		[Fact]
		public async Task VirtualPropertyGetter_WhenBaseThrows_ShouldStillExecuteOnGetCallback()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();
			int callCount = 0;
			sut.Mock.Setup.Value.OnGet.Do(() => callCount++);

			void Act() => _ = sut.Value;

			await That(Act).Throws<InvalidOperationException>();
			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task VirtualPropertySetter_WhenBaseThrows_ShouldStillExecuteOnSetCallback()
		{
			ThrowingBaseService sut = ThrowingBaseService.CreateMock();
			int receivedValue = 0;
			sut.Mock.Setup.Value.OnSet.Do(v => receivedValue = v);

			void Act() => sut.Value = 42;

			await That(Act).Throws<InvalidOperationException>();
			await That(receivedValue).IsEqualTo(42);
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
}
