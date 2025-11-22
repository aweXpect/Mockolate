namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class CallbackWithMockSubjectTests
	{
		public class ReturnMethodWith0Parameters
		{
			[Fact]
			public async Task CallbackWithMockSubject_ShouldReceiveMockSubject()
			{
				IReturnMethodSetupTest? receivedMock = null;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do<IReturnMethodSetupTest>(mock => { receivedMock = mock; })
					.Returns("a");

				sut.Method0();

				await That(receivedMock).IsSameAs(sut);
			}

			[Fact]
			public async Task CallbackWithMockSubjectAndInvocationCount_ShouldReceiveBoth()
			{
				IReturnMethodSetupTest? receivedMock = null;
				int receivedCount = -1;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do<IReturnMethodSetupTest>((mock, count) =>
					{
						receivedMock = mock;
						receivedCount = count;
					})
					.Returns("a");

				sut.Method0();

				await That(receivedMock).IsSameAs(sut);
				await That(receivedCount).IsEqualTo(0);
			}
		}

		public class ReturnMethodWith1Parameters
		{
			[Fact]
			public async Task CallbackWithMockSubject_ShouldReceiveMockSubjectAndParameter()
			{
				IReturnMethodSetupTest? receivedMock = null;
				int receivedValue = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(Any<int>())
					.Do<IReturnMethodSetupTest>((mock, v) =>
					{
						receivedMock = mock;
						receivedValue = v;
					});

				sut.Method1(42);

				await That(receivedMock).IsSameAs(sut);
				await That(receivedValue).IsEqualTo(42);
			}

			[Fact]
			public async Task CallbackWithMockSubjectAndInvocationCount_ShouldReceiveAll()
			{
				IReturnMethodSetupTest? receivedMock = null;
				int receivedCount = -1;
				int receivedValue = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(Any<int>())
					.Do<IReturnMethodSetupTest>((mock, count, v) =>
					{
						receivedMock = mock;
						receivedCount = count;
						receivedValue = v;
					});

				sut.Method1(42);

				await That(receivedMock).IsSameAs(sut);
				await That(receivedCount).IsEqualTo(0);
				await That(receivedValue).IsEqualTo(42);
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task CallbackWithMockSubject_ShouldReceiveMockSubject()
			{
				IVoidMethodSetupTest? receivedMock = null;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do<IVoidMethodSetupTest>(mock => { receivedMock = mock; });

				sut.Method0();

				await That(receivedMock).IsSameAs(sut);
			}

			[Fact]
			public async Task CallbackWithMockSubjectAndInvocationCount_ShouldReceiveBoth()
			{
				IVoidMethodSetupTest? receivedMock = null;
				int receivedCount = -1;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do<IVoidMethodSetupTest>((mock, count) =>
					{
						receivedMock = mock;
						receivedCount = count;
					});

				sut.Method0();

				await That(receivedMock).IsSameAs(sut);
				await That(receivedCount).IsEqualTo(0);
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task CallbackWithMockSubject_ShouldReceiveMockSubjectAndParameter()
			{
				IVoidMethodSetupTest? receivedMock = null;
				int receivedValue = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(Any<int>())
					.Do<IVoidMethodSetupTest>((mock, v) =>
					{
						receivedMock = mock;
						receivedValue = v;
					});

				sut.Method1(42);

				await That(receivedMock).IsSameAs(sut);
				await That(receivedValue).IsEqualTo(42);
			}

			[Fact]
			public async Task CallbackWithMockSubjectAndInvocationCount_ShouldReceiveAll()
			{
				IVoidMethodSetupTest? receivedMock = null;
				int receivedCount = -1;
				int receivedValue = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(Any<int>())
					.Do<IVoidMethodSetupTest>((mock, count, v) =>
					{
						receivedMock = mock;
						receivedCount = count;
						receivedValue = v;
					});

				sut.Method1(42);

				await That(receivedMock).IsSameAs(sut);
				await That(receivedCount).IsEqualTo(0);
				await That(receivedValue).IsEqualTo(42);
			}
		}
	}
}
