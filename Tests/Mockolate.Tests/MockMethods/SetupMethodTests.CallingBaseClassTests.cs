namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class CallingBaseClassTests
	{
		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWith1Parameter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyReturnMethodWith1Parameter(Any<int>()).CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWith1Parameter(1);

			await That(mock.MyReturnMethodWith1ParameterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWith2Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyReturnMethodWith2Parameters(Any<int>(), Any<int>()).CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWith2Parameters(1, 2);

			await That(mock.MyReturnMethodWith2ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWith3Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyReturnMethodWith3Parameters(Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWith3Parameters(1, 2, 3);

			await That(mock.MyReturnMethodWith3ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWith4Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyReturnMethodWith4Parameters(Any<int>(), Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWith4Parameters(1, 2, 3, 4);

			await That(mock.MyReturnMethodWith4ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWith5Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method
				.MyReturnMethodWith5Parameters(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWith5Parameters(1, 2, 3, 4, 5);

			await That(mock.MyReturnMethodWith5ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyReturnMethodWithoutParameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyReturnMethodWithoutParameters().CallingBaseClass(callBaseClass);

			mock.MyReturnMethodWithoutParameters();

			await That(mock.MyReturnMethodWithoutParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWith1Parameter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyVoidMethodWith1Parameter(Any<int>()).CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWith1Parameter(1);

			await That(mock.MyVoidMethodWith1ParameterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWith2Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyVoidMethodWith2Parameters(Any<int>(), Any<int>()).CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWith2Parameters(1, 2);

			await That(mock.MyVoidMethodWith2ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWith3Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyVoidMethodWith3Parameters(Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWith3Parameters(1, 2, 3);

			await That(mock.MyVoidMethodWith3ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWith4Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyVoidMethodWith4Parameters(Any<int>(), Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWith4Parameters(1, 2, 3, 4);

			await That(mock.MyVoidMethodWith4ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWith5Parameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method
				.MyVoidMethodWith5Parameters(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
				.CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWith5Parameters(1, 2, 3, 4, 5);

			await That(mock.MyVoidMethodWith5ParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task MyVoidMethodWithoutParameters_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyVoidMethodWithoutParameters().CallingBaseClass(callBaseClass);

			mock.MyVoidMethodWithoutParameters();

			await That(mock.MyVoidMethodWithoutParametersCallCount).IsEqualTo(expectedCallCount);
		}

		[Fact]
		public async Task SetupCallingBaseClassWithoutReturn_ShouldReturnBaseValue()
		{
			MyMethodService mock = Mock.Create<MyMethodService>();
			mock.SetupMock.Method.MyMethodReturning2().CallingBaseClass();

			int result = mock.MyMethodReturning2();

			await That(result).IsEqualTo(2);
		}

		public class MyMethodService
		{
			public int MyVoidMethodWithoutParametersCallCount { get; private set; }
			public int MyVoidMethodWith1ParameterCallCount { get; private set; }
			public int MyVoidMethodWith2ParametersCallCount { get; private set; }
			public int MyVoidMethodWith3ParametersCallCount { get; private set; }
			public int MyVoidMethodWith4ParametersCallCount { get; private set; }
			public int MyVoidMethodWith5ParametersCallCount { get; private set; }
			public int MyReturnMethodWithoutParametersCallCount { get; private set; }
			public int MyReturnMethodWith1ParameterCallCount { get; private set; }
			public int MyReturnMethodWith2ParametersCallCount { get; private set; }
			public int MyReturnMethodWith3ParametersCallCount { get; private set; }
			public int MyReturnMethodWith4ParametersCallCount { get; private set; }
			public int MyReturnMethodWith5ParametersCallCount { get; private set; }

			public virtual void MyVoidMethodWithoutParameters()
				=> MyVoidMethodWithoutParametersCallCount++;

			public virtual void MyVoidMethodWith1Parameter(int p1)
				=> MyVoidMethodWith1ParameterCallCount++;

			public virtual void MyVoidMethodWith2Parameters(int p1, int p2)
				=> MyVoidMethodWith2ParametersCallCount++;

			public virtual void MyVoidMethodWith3Parameters(int p1, int p2, int p3)
				=> MyVoidMethodWith3ParametersCallCount++;

			public virtual void MyVoidMethodWith4Parameters(int p1, int p2, int p3, int p4)
				=> MyVoidMethodWith4ParametersCallCount++;

			public virtual void MyVoidMethodWith5Parameters(int p1, int p2, int p3, int p4, int p5)
				=> MyVoidMethodWith5ParametersCallCount++;


			public virtual int MyReturnMethodWithoutParameters()
				=> MyReturnMethodWithoutParametersCallCount++;

			public virtual int MyReturnMethodWith1Parameter(int p1)
				=> MyReturnMethodWith1ParameterCallCount++;

			public virtual int MyReturnMethodWith2Parameters(int p1, int p2)
				=> MyReturnMethodWith2ParametersCallCount++;

			public virtual int MyReturnMethodWith3Parameters(int p1, int p2, int p3)
				=> MyReturnMethodWith3ParametersCallCount++;

			public virtual int MyReturnMethodWith4Parameters(int p1, int p2, int p3, int p4)
				=> MyReturnMethodWith4ParametersCallCount++;

			public virtual int MyReturnMethodWith5Parameters(int p1, int p2, int p3, int p4, int p5)
				=> MyReturnMethodWith5ParametersCallCount++;

			public virtual int MyMethodReturning2() => 2;
		}
	}
}
