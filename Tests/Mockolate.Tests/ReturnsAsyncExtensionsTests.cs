namespace Mockolate.Tests;

public sealed class ReturnsAsyncExtensionsTests
{
	public sealed class TaskTests
	{
		[Fact]
		public async Task ReturnsAsync_1Parameter_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method1(It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.Method1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_CallbackWithValue_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method1(It.IsAny<int>()).ReturnsAsync(v1 => v1 + 10);

			int result = await sut.Method1(2);

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method1(It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.Method1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method1(It.IsAny<int>()).ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.Method1(1);
			int result2 = await sut.Method1(1);
			int result3 = await sut.Method1(1);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync((v1, v2) => v1 + v2 + 10);

			int result = await sut.Method2(1, 2);

			await That(result).IsEqualTo(13);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42).When(i => i > 0)
				.Only(1);

			int result1 = await sut.Method2(1, 2);
			int result2 = await sut.Method2(1, 2);
			int result3 = await sut.Method2(1, 2);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.Method3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync((v1, v2, v3) => v1 + v2 + v3 + 10);

			int result = await sut.Method3(1, 2, 3);

			await That(result).IsEqualTo(16);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.Method3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42)
				.When(i => i > 0).Only(1);

			int result1 = await sut.Method3(1, 2, 3);
			int result2 = await sut.Method3(1, 2, 3);
			int result3 = await sut.Method3(1, 2, 3);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(() => 42);

			int result = await sut.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync((v1, v2, v3, v4) => v1 + v2 + v3 + v4 + 10);

			int result = await sut.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(20);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(42);

			int result = await sut.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.Method4(1, 2, 3, 4);
			int result2 = await sut.Method4(1, 2, 3, 4);
			int result3 = await sut.Method4(1, 2, 3, 4);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method0().ReturnsAsync(() => 42);

			int result = await sut.Method0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method0().ReturnsAsync(42);

			int result = await sut.Method0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method0().ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.Method0();
			int result2 = await sut.Method0();
			int result3 = await sut.Method0();

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_WithParameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(Match.AnyParameters()).ReturnsAsync(() => 42);

			int result = await sut.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_WithParameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.Method2(Match.AnyParameters()).ReturnsAsync(42);

			int result = await sut.Method2(1, 2);

			await That(result).IsEqualTo(42);
		}
	}

#if NET8_0_OR_GREATER
	public sealed class ValueTaskTests
	{
		[Fact]
		public async Task ReturnsAsync_1Parameter_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT1(It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.MethodVT1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_CallbackWithValue_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT1(It.IsAny<int>()).ReturnsAsync(v1 => v1 + 10);

			int result = await sut.MethodVT1(2);

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT1(It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.MethodVT1(1);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_1Parameter_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT1(It.IsAny<int>()).ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.MethodVT1(1);
			int result2 = await sut.MethodVT1(1);
			int result3 = await sut.MethodVT1(1);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync((v1, v2) => v1 + v2 + 10);

			int result = await sut.MethodVT2(1, 2);

			await That(result).IsEqualTo(13);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_2Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42).When(i => i > 0)
				.Only(1);

			int result1 = await sut.MethodVT2(1, 2);
			int result2 = await sut.MethodVT2(1, 2);
			int result3 = await sut.MethodVT2(1, 2);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42);

			int result = await sut.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync((v1, v2, v3) => v1 + v2 + v3 + 10);

			int result = await sut.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(16);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(42);

			int result = await sut.MethodVT3(1, 2, 3);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_3Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).ReturnsAsync(() => 42)
				.When(i => i > 0).Only(1);

			int result1 = await sut.MethodVT3(1, 2, 3);
			int result2 = await sut.MethodVT3(1, 2, 3);
			int result3 = await sut.MethodVT3(1, 2, 3);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(() => 42);

			int result = await sut.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_CallbackWithValues_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync((v1, v2, v3, v4) => v1 + v2 + v3 + v4 + 10);

			int result = await sut.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(20);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(42);

			int result = await sut.MethodVT4(1, 2, 3, 4);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_4Parameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.MethodVT4(1, 2, 3, 4);
			int result2 = await sut.MethodVT4(1, 2, 3, 4);
			int result3 = await sut.MethodVT4(1, 2, 3, 4);

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT0().ReturnsAsync(() => 42);

			int result = await sut.MethodVT0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT0().ReturnsAsync(42);

			int result = await sut.MethodVT0();

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_NoParameters_ShouldSupportWhenAndFor()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT0().ReturnsAsync(42).When(i => i > 0).Only(1);

			int result1 = await sut.MethodVT0();
			int result2 = await sut.MethodVT0();
			int result3 = await sut.MethodVT0();

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(42);
			await That(result3).IsEqualTo(0);
		}

		[Fact]
		public async Task ReturnsAsync_WithParameters_Callback_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(Match.AnyParameters()).ReturnsAsync(() => 42);

			int result = await sut.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReturnsAsync_WithParameters_ReturnsConfiguredValue()
		{
			IReturnsAsyncExtensionsSetupTest sut = Mock.Create<IReturnsAsyncExtensionsSetupTest>();
			sut.SetupMock.Method.MethodVT2(Match.AnyParameters()).ReturnsAsync(42);

			int result = await sut.MethodVT2(1, 2);

			await That(result).IsEqualTo(42);
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
