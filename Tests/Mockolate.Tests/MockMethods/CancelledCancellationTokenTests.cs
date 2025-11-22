using System.Threading;
using System.Threading.Tasks;
using static Mockolate.Match;

namespace Mockolate.Tests.MockMethods;

public sealed class CancelledCancellationTokenTests
{
	public sealed class AsyncMethods
	{
		[Fact]
		public async Task WithTask_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Task result = sut.TaskMethod(cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithTaskOfInt_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Task<int> result = sut.TaskOfIntMethod(cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithTaskOfString_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Task<string> result = sut.TaskOfStringMethod(cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithValueTask_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			ValueTask result = sut.ValueTaskMethod(cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithValueTaskOfInt_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			ValueTask<int> result = sut.ValueTaskOfIntMethod(cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}

		[Fact]
		public async Task WithNonCancelledToken_ShouldReturnDefaultValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken nonCancelledToken = new(false);

			Task result = sut.TaskMethod(nonCancelledToken);

			await That(result.IsCompleted).IsTrue();
			await That(result.IsCanceled).IsFalse();
		}

		[Fact]
		public async Task WithSetup_ShouldUseSetupValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);
			sut.SetupMock.Method.TaskOfIntMethod(Any<CancellationToken>()).Returns(Task.FromResult(42));

			Task<int> result = sut.TaskOfIntMethod(cancelledToken);

			await That(result.IsCompleted).IsTrue();
			await That(result.IsCanceled).IsFalse();
			await That(await result).IsEqualTo(42);
		}

		[Fact]
		public async Task WithMultipleParameters_WhenOneIsCancelled_ShouldReturnCancelledTask()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Task<int> result = sut.MultipleParametersMethod("test", 123, cancelledToken);

			await That(result.IsCanceled).IsTrue();
		}
	}

	public sealed class SynchronousMethods
	{
		[Fact]
		public async Task WithVoidMethod_ShouldThrowOperationCanceledException()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Exception? exception = Record.Exception(() => sut.VoidMethod(cancelledToken));

			await That(exception).IsNotNull();
		}

		[Fact]
		public async Task WithIntMethod_ShouldThrowOperationCanceledException()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);

			Exception? exception = Record.Exception(() => sut.IntMethod(cancelledToken));

			await That(exception).IsNotNull();
		}

		[Fact]
		public async Task WithNonCancelledToken_ShouldReturnDefaultValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken nonCancelledToken = new(false);

			int result = sut.IntMethod(nonCancelledToken);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task WithSetup_ShouldUseSetupValue()
		{
			IMockWithCancellationToken sut = Mock.Create<IMockWithCancellationToken>();
			CancellationToken cancelledToken = new(true);
			sut.SetupMock.Method.IntMethod(Any<CancellationToken>()).Returns(42);

			int result = sut.IntMethod(cancelledToken);

			await That(result).IsEqualTo(42);
		}
	}

	public interface IMockWithCancellationToken
	{
		Task TaskMethod(CancellationToken cancellationToken);
		Task<int> TaskOfIntMethod(CancellationToken cancellationToken);
		Task<string> TaskOfStringMethod(CancellationToken cancellationToken);
		ValueTask ValueTaskMethod(CancellationToken cancellationToken);
		ValueTask<int> ValueTaskOfIntMethod(CancellationToken cancellationToken);
		Task<int> MultipleParametersMethod(string param1, int param2, CancellationToken cancellationToken);
		void VoidMethod(CancellationToken cancellationToken);
		int IntMethod(CancellationToken cancellationToken);
	}
}
