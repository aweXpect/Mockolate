using System.Collections.Generic;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class ReturnsThrowsTests
	{
		[Fact]
		public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(With(1))
				.Returns("a")
				.Returns("b");

			string result11 = mock[1];
			string result2 = mock[2];
			string result12 = mock[1];
			string result13 = mock[1];
			string result14 = mock[1];
			string result15 = mock[1];

			await That(result11).IsEqualTo("a");
			await That(result2).IsEqualTo("");
			await That(result12).IsEqualTo("b");
			await That(result13).IsEqualTo("a");
			await That(result14).IsEqualTo("b");
			await That(result15).IsEqualTo("a");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("a")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut[1];
			Exception? result2 = Record.Exception(() => _ = sut[2]);
			string result3 = sut[3];

			await That(result1).IsEqualTo("a");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("a")
				.Returns(() => "b")
				.Returns(p1 => $"foo-{p1}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut[i];
			}

			await That(result).IsEqualTo(["a", "b", "foo-2", "a", "b", "foo-5", "a", "b", "foo-8", "a",]);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns(() => "foo");

			string result = sut[1];

			await That(result).IsEqualTo("foo");
		}

		[Fact]
		public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.InitializeWith("a")
				.Returns(p1 => $"foo-{p1}");

			string result = sut[3];

			await That(result).IsEqualTo("foo-3");
		}

		[Fact]
		public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.InitializeWith("init")
				.Returns((v, p1) => $"foo-{v}-{p1}");

			string result = sut[3];

			await That(result).IsEqualTo("foo-init-3");
		}

		[Fact]
		public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("foo").For(2)
				.Returns("bar").For(3);

			List<string> values = [];
			for (int i = 0; i < 10; i++)
			{
				values.Add(sut[i]);
			}

			await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
		}

		[Fact]
		public async Task Returns_Forever_ShouldUseTheLastValueForever()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("a")
				.Returns("b")
				.Returns("c").Forever();

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut[i];
			}

			await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("foo");

			string result = sut[3];

			await That(result).IsEqualTo("foo");
		}

		[Fact]
		public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("foo").When(i => i > 0);

			string result1 = sut[3];
			string result2 = sut[4];
			string result3 = sut[5];

			await That(result1).IsEqualTo("");
			await That(result2).IsEqualTo("foo");
			await That(result3).IsEqualTo("foo");
		}

		[Fact]
		public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Returns("foo").When(i => i > 0).For(2)
				.Returns("baz")
				.Returns("bar").For(3);

			List<string> values = [];
			for (int i = 0; i < 10; i++)
			{
				values.Add(sut[i]);
			}

			await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			string result = sut[2];

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task Throws_Callback_ShouldThrowException()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
			{
				_ = sut[3];
			}

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithParameters_ShouldThrowException()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.InitializeWith("init")
				.Throws(p1 => new Exception($"foo-{p1}"));

			void Act()
			{
				_ = sut[3];
			}

			await That(Act).ThrowsException().WithMessage("foo-3");
		}

		[Fact]
		public async Task Throws_CallbackWithParametersAndValue_ShouldThrowException()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.InitializeWith("init")
				.Throws((v, p1) => new Exception($"foo-{v}-{p1}"));

			void Act()
			{
				_ = sut[3];
			}

			await That(Act).ThrowsException().WithMessage("foo-init-3");
		}

		[Fact]
		public async Task Throws_Generic_ShouldThrowException()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Throws<ArgumentNullException>();

			void Act()
			{
				_ = sut[3];
			}

			await That(Act).ThrowsExactly<ArgumentNullException>();
		}

		[Fact]
		public async Task Throws_ShouldThrowException()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(Any<int>())
				.Throws(new Exception("foo"));

			void Act()
			{
				_ = sut[3];
			}

			await That(Act).ThrowsException().WithMessage("foo");
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With(1), Any<int>())
					.Returns("a")
					.Returns("b");

				string result11 = mock[1, 2];
				string result2 = mock[2, 2];
				string result12 = mock[1, 2];
				string result13 = mock[1, 2];
				string result14 = mock[1, 2];
				string result15 = mock[1, 2];

				await That(result11).IsEqualTo("a");
				await That(result2).IsEqualTo("");
				await That(result12).IsEqualTo("b");
				await That(result13).IsEqualTo("a");
				await That(result14).IsEqualTo("b");
				await That(result15).IsEqualTo("a");
			}

			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("a")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut[1, 1];
				Exception? result2 = Record.Exception(() => _ = sut[2, 2]);
				string result3 = sut[3, 3];

				await That(result1).IsEqualTo("a");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2) => $"foo-{p1}-{p2}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i];
				}

				await That(result).IsEqualTo(["a", "b", "foo-2-4", "a", "b", "foo-5-25", "a", "b", "foo-8-64", "a",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns(() => "foo");

				string result = sut[1, 2];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.InitializeWith("a")
					.Returns((p1, p2) => $"foo-{p1}-{p2}");

				string result = sut[3, 4];

				await That(result).IsEqualTo("foo-3-4");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2) => $"foo-{v}-{p1}-{p2}");

				string result = sut[3, 4];

				await That(result).IsEqualTo("foo-init-3-4");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2]);
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, 2];
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("foo");

				string result = sut[1, 2];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut[3, 2];
				string result2 = sut[4, 2];
				string result3 = sut[5, 2];

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2]);
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				string result = sut[1, 2];

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithParameters_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((p1, p2) => new Exception($"foo-{p1}-{p2}"));

				void Act()
				{
					_ = sut[1, 2];
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2");
			}

			[Fact]
			public async Task Throws_CallbackWithParametersAndValue_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((v, p1, p2) => new Exception($"foo-{v}-{p1}-{p2}"));

				void Act()
				{
					_ = sut[1, 2];
				}

				await That(Act).ThrowsException().WithMessage("foo-init-1-2");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					_ = sut[1, 2];
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With(1), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b");

				string result11 = mock[1, 2, 3];
				string result2 = mock[2, 2, 3];
				string result12 = mock[1, 2, 3];
				string result13 = mock[1, 2, 3];
				string result14 = mock[1, 2, 3];
				string result15 = mock[1, 2, 3];

				await That(result11).IsEqualTo("a");
				await That(result2).IsEqualTo("");
				await That(result12).IsEqualTo("b");
				await That(result13).IsEqualTo("a");
				await That(result14).IsEqualTo("b");
				await That(result15).IsEqualTo("a");
			}

			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut[1, 1, 1];
				Exception? result2 = Record.Exception(() => _ = sut[2, 2, 2]);
				string result3 = sut[3, 3, 3];

				await That(result1).IsEqualTo("a");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2, p3) => $"foo-{p1}-{p2}-{p3}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i, (i * i) + i];
				}

				await That(result)
					.IsEqualTo(["a", "b", "foo-2-4-6", "a", "b", "foo-5-25-30", "a", "b", "foo-8-64-72", "a",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3) => $"foo-{p1}-{p2}-{p3}");

				string result = sut[3, 4, 5];

				await That(result).IsEqualTo("foo-3-4-5");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3) => $"foo-{v}-{p1}-{p2}-{p3}");

				string result = sut[3, 4, 5];

				await That(result).IsEqualTo("foo-init-3-4-5");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3]);
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, 2, 3];
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("foo");

				string result = sut[1, 2, 3];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut[3, 2, 3];
				string result2 = sut[4, 2, 3];
				string result3 = sut[5, 2, 3];

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3]);
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				string result = sut[1, 2, 3];

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithParameters_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((p1, p2, p3) => new Exception($"foo-{p1}-{p2}-{p3}"));

				void Act()
				{
					_ = sut[1, 2, 3];
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3");
			}

			[Fact]
			public async Task Throws_CallbackWithParametersAndValue_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((v, p1, p2, p3) => new Exception($"foo-{v}-{p1}-{p2}-{p3}"));

				void Act()
				{
					_ = sut[1, 2, 3];
				}

				await That(Act).ThrowsException().WithMessage("foo-init-1-2-3");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					_ = sut[1, 2, 3];
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With(1), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b");

				string result11 = mock[1, 2, 3, 4];
				string result2 = mock[2, 2, 3, 4];
				string result12 = mock[1, 2, 3, 4];
				string result13 = mock[1, 2, 3, 4];
				string result14 = mock[1, 2, 3, 4];
				string result15 = mock[1, 2, 3, 4];

				await That(result11).IsEqualTo("a");
				await That(result2).IsEqualTo("");
				await That(result12).IsEqualTo("b");
				await That(result13).IsEqualTo("a");
				await That(result14).IsEqualTo("b");
				await That(result15).IsEqualTo("a");
			}

			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut[1, 1, 1, 1];
				Exception? result2 = Record.Exception(() => _ = sut[2, 2, 2, 2]);
				string result3 = sut[3, 3, 3, 3];

				await That(result1).IsEqualTo("a");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2, p3, p4) => $"foo-{p1}-{p2}-{p3}-{p4}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i, (i * i) + i, (i * i) - i];
				}

				await That(result).IsEqualTo([
					"a", "b", "foo-2-4-6-2", "a", "b", "foo-5-25-30-20", "a", "b", "foo-8-64-72-56", "a",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3, 4];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3, p4) => $"foo-{p1}-{p2}-{p3}-{p4}");

				string result = sut[3, 4, 5, 6];

				await That(result).IsEqualTo("foo-3-4-5-6");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3, p4) => $"foo-{v}-{p1}-{p2}-{p3}-{p4}");

				string result = sut[3, 4, 5, 6];

				await That(result).IsEqualTo("foo-init-3-4-5-6");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3, 4]);
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, 2, 3, 4];
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo");

				string result = sut[1, 2, 3, 4];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut[3, 2, 3, 4];
				string result2 = sut[4, 2, 3, 4];
				string result3 = sut[5, 2, 3, 4];

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3, 4]);
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				string result = sut[1, 2, 3, 4];

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3, 4];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithParameters_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((p1, p2, p3, p4) => new Exception($"foo-{p1}-{p2}-{p3}-{p4}"));

				void Act()
				{
					_ = sut[1, 2, 3, 4];
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
			}

			[Fact]
			public async Task Throws_CallbackWithParametersAndValue_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((v, p1, p2, p3, p4) => new Exception($"foo-{v}-{p1}-{p2}-{p3}-{p4}"));

				void Act()
				{
					_ = sut[1, 2, 3, 4];
				}

				await That(Act).ThrowsException().WithMessage("foo-init-1-2-3-4");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					_ = sut[1, 2, 3, 4];
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3, 4];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With(1), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b");

				string result11 = mock[1, 2, 3, 4, 5];
				string result2 = mock[2, 2, 3, 4, 5];
				string result12 = mock[1, 2, 3, 4, 5];
				string result13 = mock[1, 2, 3, 4, 5];
				string result14 = mock[1, 2, 3, 4, 5];
				string result15 = mock[1, 2, 3, 4, 5];

				await That(result11).IsEqualTo("a");
				await That(result2).IsEqualTo("");
				await That(result12).IsEqualTo("b");
				await That(result13).IsEqualTo("a");
				await That(result14).IsEqualTo("b");
				await That(result15).IsEqualTo("a");
			}

			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut[1, 1, 1, 1, 1];
				Exception? result2 = Record.Exception(() => _ = sut[2, 2, 2, 2, 2]);
				string result3 = sut[3, 3, 3, 3, 3];

				await That(result1).IsEqualTo("a");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2, p3, p4, p5) => $"foo-{p1}-{p2}-{p3}-{p4}-{p5}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i, (i * i) + i, (i * i) - i, i + i];
				}

				await That(result).IsEqualTo([
					"a", "b", "foo-2-4-6-2-4", "a", "b", "foo-5-25-30-20-10", "a", "b", "foo-8-64-72-56-16", "a",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3, 4, 5];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3, p4, p5) => $"foo-{p1}-{p2}-{p3}-{p4}-{p5}");

				string result = sut[3, 4, 5, 6, 7];

				await That(result).IsEqualTo("foo-3-4-5-6-7");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3, p4, p5) => $"foo-{v}-{p1}-{p2}-{p3}-{p4}-{p5}");

				string result = sut[3, 4, 5, 6, 7];

				await That(result).IsEqualTo("foo-init-3-4-5-6-7");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3, 4, 5]);
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, 2, 3, 4, 5];
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo");

				string result = sut[1, 2, 3, 4, 5];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut[3, 2, 3, 4, 5];
				string result2 = sut[4, 2, 3, 4, 5];
				string result3 = sut[5, 2, 3, 4, 5];

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut[i, 2, 3, 4, 5]);
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				string result = sut[1, 2, 3, 4, 5];

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3, 4, 5];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithParameters_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((p1, p2, p3, p4, p5) => new Exception($"foo-{p1}-{p2}-{p3}-{p4}-{p5}"));

				void Act()
				{
					_ = sut[1, 2, 3, 4, 5];
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_CallbackWithParametersAndValue_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.InitializeWith("init")
					.Throws((v, p1, p2, p3, p4, p5) => new Exception($"foo-{v}-{p1}-{p2}-{p3}-{p4}-{p5}"));

				void Act()
				{
					_ = sut[1, 2, 3, 4, 5];
				}

				await That(Act).ThrowsException().WithMessage("foo-init-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					_ = sut[1, 2, 3, 4, 5];
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					_ = sut[1, 2, 3, 4, 5];
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}
	}
}
