namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class ReturnsThrowsTests
	{
		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
				.Returns(() => "foo");

			string result = sut[1];

			await That(result).IsEqualTo("foo");
		}

		[Fact]
		public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(WithAny<int>())
				.InitializeWith("a")
				.Returns(p1 => $"foo-{p1}");

			string result = sut[3];

			await That(result).IsEqualTo("foo-3");
		}

		[Fact]
		public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(WithAny<int>())
				.InitializeWith("init")
				.Returns((v, p1) => $"foo-{v}-{p1}");

			string result = sut[3];

			await That(result).IsEqualTo("foo-init-3");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(WithAny<int>())
				.Returns("foo");

			string result = sut[3];

			await That(result).IsEqualTo("foo");
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

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
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

			sut.SetupMock.Indexer(WithAny<int>())
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
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.Returns(() => "foo");

				string result = sut[1, 2];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.InitializeWith("a")
					.Returns((p1, p2) => $"foo-{p1}-{p2}");

				string result = sut[3, 4];

				await That(result).IsEqualTo("foo-3-4");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2) => $"foo-{v}-{p1}-{p2}");

				string result = sut[3, 4];

				await That(result).IsEqualTo("foo-init-3-4");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.Returns("foo");

				string result = sut[1, 2];

				await That(result).IsEqualTo("foo");
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
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
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3) => $"foo-{p1}-{p2}-{p3}");

				string result = sut[3, 4, 5];

				await That(result).IsEqualTo("foo-3-4-5");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3) => $"foo-{v}-{p1}-{p2}-{p3}");

				string result = sut[3, 4, 5];

				await That(result).IsEqualTo("foo-init-3-4-5");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns("foo");

				string result = sut[1, 2, 3];

				await That(result).IsEqualTo("foo");
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
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
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2, p3, p4) => $"foo-{p1}-{p2}-{p3}-{p4}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i, (i * i) + i, (i * i) - i];
				}

				await That(result).IsEqualTo([
					"a", "b", "foo-2-4-6-2", "a", "b", "foo-5-25-30-20", "a", "b", "foo-8-64-72-56", "a"
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3, 4];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3, p4) => $"foo-{p1}-{p2}-{p3}-{p4}");

				string result = sut[3, 4, 5, 6];

				await That(result).IsEqualTo("foo-3-4-5-6");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3, p4) => $"foo-{v}-{p1}-{p2}-{p3}-{p4}");

				string result = sut[3, 4, 5, 6];

				await That(result).IsEqualTo("foo-init-3-4-5-6");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns("foo");

				string result = sut[1, 2, 3, 4];

				await That(result).IsEqualTo("foo");
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns("a")
					.Returns(() => "b")
					.Returns((p1, p2, p3, p4, p5) => $"foo-{p1}-{p2}-{p3}-{p4}-{p5}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut[i, i * i, (i * i) + i, (i * i) - i, i + i];
				}

				await That(result).IsEqualTo([
					"a", "b", "foo-2-4-6-2-4", "a", "b", "foo-5-25-30-20-10", "a", "b", "foo-8-64-72-56-16", "a"
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns(() => "foo");

				string result = sut[1, 2, 3, 4, 5];

				await That(result).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_CallbackWithParameters_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("a")
					.Returns((p1, p2, p3, p4, p5) => $"foo-{p1}-{p2}-{p3}-{p4}-{p5}");

				string result = sut[3, 4, 5, 6, 7];

				await That(result).IsEqualTo("foo-3-4-5-6-7");
			}

			[Fact]
			public async Task Returns_CallbackWithParametersAndValue_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("init")
					.Returns((v, p1, p2, p3, p4, p5) => $"foo-{v}-{p1}-{p2}-{p3}-{p4}-{p5}");

				string result = sut[3, 4, 5, 6, 7];

				await That(result).IsEqualTo("foo-init-3-4-5-6-7");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.Returns("foo");

				string result = sut[1, 2, 3, 4, 5];

				await That(result).IsEqualTo("foo");
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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

				sut.SetupMock.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
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
