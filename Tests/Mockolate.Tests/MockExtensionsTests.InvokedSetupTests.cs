using Mockolate.Exceptions;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests;

public sealed partial class MockExtensionsTests
{
	public sealed class InvokedSetupTests
	{
		[Test]
		public async Task WhenMethodSetupIsNotVerifiable_ShouldThrowMockException()
		{
			MyServiceBase mock = Mock.Create<MyServiceBase>();
			IMethodSetup setup = new MyMethodSetup();

			void Act()
			{
				mock.VerifyMock.InvokedSetup(setup).Never();
			}

			await That(Act).Throws<MockException>()
				.WithMessage("The setup is not verifiable.");
		}

		[Test]
		public async Task WhenSubjectIsNoMock_ShouldThrowMockException()
		{
			MyServiceBase mock = Mock.Create<MyServiceBase>();
			IMockVerify<MyServiceBase> verify = new MyMockVerify<MyServiceBase>();
			IMethodSetup setup = mock.SetupMock.Method.DoSomething(Match.AnyParameters());

			void Act()
			{
				verify.InvokedSetup(setup).Never();
			}

			await That(Act).Throws<MockException>()
				.WithMessage("The subject is no mock subject.");
		}

		private sealed class MyMockVerify<T> : IMockVerify<T>
		{
			public bool ThatAllInteractionsAreVerified()
				=> throw new NotSupportedException();

			public bool ThatAllSetupsAreUsed()
				=> throw new NotSupportedException();
		}

		private sealed class MyMethodSetup : IMethodSetup
		{
		}
	}
}
