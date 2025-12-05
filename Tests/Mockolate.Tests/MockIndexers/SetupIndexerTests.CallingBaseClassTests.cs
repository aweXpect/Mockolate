namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class CallingBaseClassTests
	{
		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith1Key_Getter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>()).CallingBaseClass(callBaseClass);

			_ = mock[1];

			await That(mock.MyIndexerWith1KeyGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith1Key_Setter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>()).CallingBaseClass(callBaseClass);

			mock[1] = 1;

			await That(mock.MyIndexerWith1KeySetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith2Keys_Getter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			_ = mock[1, 2];

			await That(mock.MyIndexerWith2KeysGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith2Keys_Setter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			mock[1, 2] = 1;

			await That(mock.MyIndexerWith2KeysSetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith3Keys_Getter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			_ = mock[1, 2, 3];

			await That(mock.MyIndexerWith3KeysGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith3Keys_Setter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			mock[1, 2, 3] = 1;

			await That(mock.MyIndexerWith3KeysSetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith4Keys_Getter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			_ = mock[1, 2, 3, 4];

			await That(mock.MyIndexerWith4KeysGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith4Keys_Setter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()).CallingBaseClass(callBaseClass);

			mock[1, 2, 3, 4] = 1;

			await That(mock.MyIndexerWith4KeysSetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith5Keys_Getter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.CallingBaseClass(callBaseClass);

			_ = mock[1, 2, 3, 4, 5];

			await That(mock.MyIndexerWith5KeysGetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 1)]
		public async Task IndexerWith5Keys_Setter_ShouldCallBaseWhenRequested(bool callBaseClass,
			int expectedCallCount)
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.CallingBaseClass(callBaseClass);

			mock[1, 2, 3, 4, 5] = 1;

			await That(mock.MyIndexerWith5KeysSetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Fact]
		public async Task SetupCallingBaseClassWithoutReturn_ShouldReturnBaseValue()
		{
			MyIndexerService mock = Mock.Create<MyIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<string>())
				.CallingBaseClass();

			int result = mock["returning 2"];

			await That(result).IsEqualTo(2);
		}

		public class MyIndexerService
		{
			public int MyIndexerWith1KeyGetterCallCount { get; private set; }
			public int MyIndexerWith1KeySetterCallCount { get; private set; }
			public int MyIndexerWith2KeysGetterCallCount { get; private set; }
			public int MyIndexerWith2KeysSetterCallCount { get; private set; }
			public int MyIndexerWith3KeysGetterCallCount { get; private set; }
			public int MyIndexerWith3KeysSetterCallCount { get; private set; }
			public int MyIndexerWith4KeysGetterCallCount { get; private set; }
			public int MyIndexerWith4KeysSetterCallCount { get; private set; }
			public int MyIndexerWith5KeysGetterCallCount { get; private set; }
			public int MyIndexerWith5KeysSetterCallCount { get; private set; }

			public virtual int this[int key1]
			{
				get => MyIndexerWith1KeyGetterCallCount++;
				set => MyIndexerWith1KeySetterCallCount += value;
			}

			public virtual int this[int key1, int key2]
			{
				get => MyIndexerWith2KeysGetterCallCount++;
				set => MyIndexerWith2KeysSetterCallCount += value;
			}

			public virtual int this[int key1, int key2, int key3]
			{
				get => MyIndexerWith3KeysGetterCallCount++;
				set => MyIndexerWith3KeysSetterCallCount += value;
			}

			public virtual int this[int key1, int key2, int key3, int key4]
			{
				get => MyIndexerWith4KeysGetterCallCount++;
				set => MyIndexerWith4KeysSetterCallCount += value;
			}

			public virtual int this[int key1, int key2, int key3, int key4, int key5]
			{
				get => MyIndexerWith5KeysGetterCallCount++;
				set => MyIndexerWith5KeysSetterCallCount += value;
			}

			public virtual int this[string keyReturning2]
				=> 2;
		}
	}
}
