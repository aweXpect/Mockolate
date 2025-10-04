using static Mockolate.Tests.Setup.ReturnMethodSetupTests;

namespace Mockolate.Tests;

public sealed class ReturnsAsyncExtensionsTests
{
	public sealed class TaskTests
	{

		[Fact]
		public async Task ReturnsAsync_NoParameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method0().ReturnsAsync(42);

			int result = await sut.Object.Method0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method0().ReturnsAsync(() => 42);

			int result = await sut.Object.Method0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method1(With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.Method1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method1(With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.Method1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_CallbackWithValue_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method1(With.Any<int>()).ReturnsAsync(v1 => v1 + 10);

			int result = await sut.Object.Method1(2);

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2) => v1 + v2 + 10);

			int result = await sut.Object.Method2(1, 2);

			await That(result).IsEqualTo(13);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.Method3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.Method3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2, v3) => v1 + v2 + v3 + 10);

			int result = await sut.Object.Method3(1, 2, 3);

			await That(result).IsEqualTo(16);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2, v3, v4) => v1 + v2 + v3 + v4 + 10);

			int result = await sut.Object.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(20);
		}
	}

#if NET8_0_OR_GREATER
	public sealed class ValueTaskTests
	{
		[Fact]
		public async Task ReturnsAsync_NoParameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT0().ReturnsAsync(42);

			int result = await sut.Object.MethodVT0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT0().ReturnsAsync(() => 42);

			int result = await sut.Object.MethodVT0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT1(With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.MethodVT1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT1(With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.MethodVT1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_CallbackWithValue_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT1(With.Any<int>()).ReturnsAsync(v1 => v1 + 10);

			int result = await sut.Object.MethodVT1(2);

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT2(With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT2(With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT2(With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2) => v1 + v2 + 10);

			int result = await sut.Object.MethodVT2(1, 2);

			await That(result).IsEqualTo(13);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT3(With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2, v3) => v1 + v2 + v3 + 10);

			int result = await sut.Object.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(16);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(42);

			int result = await sut.Object.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_Callback_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync(() => 42);

			int result = await sut.Object.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			Mock<IReturnsAsyncExtensionsSetupTest> sut = Mock.For<IReturnsAsyncExtensionsSetupTest>();
			sut.Setup.MethodVT4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>()).ReturnsAsync((v1, v2, v3, v4) => v1 + v2 + v3 + v4 + 10);

			int result = await sut.Object.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(20);
		}
	}
#endif

	public interface IReturnsAsyncExtensionsSetupTest
	{
		Task<int> Method0();
		Task<int> Method1(int p1);
		Task<int> Method2(int p1, int p2);
		Task<int> Method3(int p1, int p2, int p3);
		Task<int> Method4(int p1, int p2, int p3, int p4);
#if NET8_0_OR_GREATER
		ValueTask<int> MethodVT0();
		ValueTask<int> MethodVT1(int p1);
		ValueTask<int> MethodVT2(int p1, int p2);
		ValueTask<int> MethodVT3(int p1, int p2, int p3);
		ValueTask<int> MethodVT4(int p1, int p2, int p3, int p4);
#endif
	}
}
