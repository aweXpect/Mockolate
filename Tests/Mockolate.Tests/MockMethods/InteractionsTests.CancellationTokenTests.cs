using System.Threading;

namespace Mockolate.Tests.MockMethods;

public sealed partial class InteractionsTests
{
	public sealed class CancellationTokenTests
	{
		[Fact]
		public async Task WithMultipleParameters_WhenOneIsCanceled_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			Task<int> result = sut.MultipleParametersMethod("test", 123, canceledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithNonCanceledToken_ShouldReturnDefaultValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken nonCanceledToken = new(false);

			Task result = sut.TaskMethod(nonCanceledToken);

			await That(result.IsCompleted).IsTrue();
			await That(result.IsCanceled).IsFalse();
		}

		[Fact]
		public async Task WithSetup_ShouldUseSetupValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);
			sut.SetupMock.Method.TaskOfIntMethod(Any<CancellationToken>()).Returns(Task.FromResult(42));

			Task<int> result = sut.TaskOfIntMethod(canceledToken);

			await That(result.IsCompleted).IsTrue();
			await That(result.IsCanceled).IsFalse();
			await That(await result).IsEqualTo(42);
		}

		[Fact]
		public async Task WithTask_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			Task result = sut.TaskMethod(canceledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithTaskOfInt_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			Task<int> result = sut.TaskOfIntMethod(canceledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithTaskOfString_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			Task<string> result = sut.TaskOfStringMethod(canceledToken);

			await That(result.IsCanceled).IsTrue();
		}

#if NET8_0_OR_GREATER
		[Fact]
		public async Task WithValueTask_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			ValueTask result = sut.ValueTaskMethod(canceledToken);

			await That(result.IsCanceled).IsTrue();
		}
#endif

#if NET8_0_OR_GREATER
		[Fact]
		public async Task WithValueTaskOfInt_ShouldReturnCanceledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken canceledToken = new(true);

			ValueTask<int> result = sut.ValueTaskOfIntMethod(canceledToken);

			await That(result.IsCanceled).IsTrue();
		}
#endif
	}

	public interface IMockWithCancellationToken
	{
		Task TaskMethod(CancellationToken cancellationToken);
		Task<int> TaskOfIntMethod(CancellationToken cancellationToken);
		Task<string> TaskOfStringMethod(CancellationToken cancellationToken);
		ValueTask ValueTaskMethod(CancellationToken cancellationToken);
		ValueTask<int> ValueTaskOfIntMethod(CancellationToken cancellationToken);
		Task<int> MultipleParametersMethod(string param1, int param2, CancellationToken cancellationToken);
	}
}
