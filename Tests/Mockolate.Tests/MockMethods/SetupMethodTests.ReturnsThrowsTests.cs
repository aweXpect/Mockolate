namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class ReturnsThrowsTests
	{
		public class ReturnMethodWith0Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0()
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method0();
				Exception? result2 = Record.Exception(() => sut.Subject.Method0());
				string result3 = sut.Subject.Method0();

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0()
					.Returns("d")
					.Returns("c")
					.Returns(() => "b");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method0();
				}

				await That(result).IsEqualTo(["d", "c", "b", "d", "c", "b", "d", "c", "b", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0().Returns(() => "d");

				string result = sut.Subject.Method0();

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0().Returns("d");

				string result = sut.Subject.Method0();

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method0();

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0().Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method0();

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method0().Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method0();

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class ReturnMethodWith1Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method1(1);
				Exception? result2 = Record.Exception(() => sut.Subject.Method1(2));
				string result3 = sut.Subject.Method1(3);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns(v => $"foo-{v}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method1(i);
				}

				await That(result).IsEqualTo(["d", "c", "foo-2", "d", "c", "foo-5", "d", "c", "foo-8", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Returns(() => "d");

				string result = sut.Subject.Method1(3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Returns(x => $"foo-{x}");

				string result = sut.Subject.Method1(3);

				await That(result).IsEqualTo("foo-3");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Returns("d");

				string result = sut.Subject.Method1(3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method1(3);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method1(1);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Throws(v1 => new Exception("foo-" + v1));

				void Act()
					=> sut.Subject.Method1(42);

				await That(Act).ThrowsException().WithMessage("foo-42");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method1(1);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Callback(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Subject.Method1(1);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}
		}

		public class ReturnMethodWith2Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method2(1, 2);
				Exception? result2 = Record.Exception(() => sut.Subject.Method2(2, 3));
				string result3 = sut.Subject.Method2(3, 4);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2) => $"foo-{v1}-{v2}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method2(i, 2 * i);
				}

				await That(result).IsEqualTo(["d", "c", "foo-2-4", "d", "c", "foo-5-10", "d", "c", "foo-8-16", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns(() => "d");

				string result = sut.Subject.Method2(2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns((x, y) => $"foo-{x}-{y}");

				string result = sut.Subject.Method2(2, 3);

				await That(result).IsEqualTo("foo-2-3");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns("d");

				string result = sut.Subject.Method2(2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method2(2, 3);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo-1-2");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Subject.Method2(1, 2);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}
		}

		public class ReturnMethodWith3Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method3(1, 2, 3);
				Exception? result2 = Record.Exception(() => sut.Subject.Method3(2, 3, 4));
				string result3 = sut.Subject.Method3(3, 4, 5);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method3(i, 2 * i, 3 * i);
				}

				await That(result)
					.IsEqualTo(["d", "c", "foo-2-4-6", "d", "c", "foo-5-10-15", "d", "c", "foo-8-16-24", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns(() => "d");

				string result = sut.Subject.Method3(1, 2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns((x, y, z) => $"foo-{x}-{y}-{z}");

				string result = sut.Subject.Method3(2, 3, 4);

				await That(result).IsEqualTo("foo-2-3-4");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d");

				string result = sut.Subject.Method3(1, 2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method3(2, 3, 4);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Subject.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}
		}

		public class ReturnMethodWith4Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method4(1, 2, 3, 4);
				Exception? result2 = Record.Exception(() => sut.Subject.Method4(2, 3, 4, 5));
				string result3 = sut.Subject.Method4(3, 4, 5, 6);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3, v4) => $"foo-{v1}-{v2}-{v3}-{v4}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(result).IsEqualTo([
					"d", "c", "foo-2-4-6-8", "d", "c", "foo-5-10-15-20", "d", "c", "foo-8-16-24-32", "d",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns(() => "d");

				string result = sut.Subject.Method4(1, 2, 3, 4);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns((x, y, z, a) => $"foo-{x}-{y}-{z}-{a}");

				string result = sut.Subject.Method4(2, 3, 4, 5);

				await That(result).IsEqualTo("foo-2-3-4-5");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Returns("d");

				string result = sut.Subject.Method4(1, 2, 3, 4);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method4(2, 3, 4, 5);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Subject.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}
		}

		public class ReturnMethodWith5Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Subject.Method5(1, 2, 3, 4, 5);
				Exception? result2 = Record.Exception(() => sut.Subject.Method5(2, 3, 4, 5, 6));
				string result3 = sut.Subject.Method5(3, 4, 5, 6, 7);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3, v4, v5) => $"foo-{v1}-{v2}-{v3}-{v4}-{v5}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Subject.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(result).IsEqualTo([
					"d", "c", "foo-2-4-6-8-10", "d", "c", "foo-5-10-15-20-25", "d", "c", "foo-8-16-24-32-40", "d",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Returns(() => "d");

				string result = sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Returns((x, y, z, a, b) => $"foo-{x}-{y}-{z}-{a}-{b}");

				string result = sut.Subject.Method5(2, 3, 4, 5, 6);

				await That(result).IsEqualTo("foo-2-3-4-5-6");
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Returns("d");

				string result = sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Subject.Method5(2, 3, 4, 5, 6);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Callback(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0()
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method0();
				Exception? result1 = Record.Exception(() => sut.Subject.Method0());
				sut.Subject.Method0();
				sut.Subject.Method0();
				Exception? result2 = Record.Exception(() => sut.Subject.Method0());
				sut.Subject.Method0();

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0().Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method0();

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0().Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method0();

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method1(1);
				Exception? result1 = Record.Exception(() => sut.Subject.Method1(2));
				sut.Subject.Method1(3);
				sut.Subject.Method1(4);
				Exception? result2 = Record.Exception(() => sut.Subject.Method1(5));
				sut.Subject.Method1(6);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method1(1);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Throws(v1 => new Exception($"foo-{v1}"));

				void Act()
					=> sut.Subject.Method1(1);

				await That(Act).ThrowsException().WithMessage("foo-1");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>()).Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method1(1);

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class VoidMethodWith2Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method2(1, 2);
				Exception? result1 = Record.Exception(() => sut.Subject.Method2(2, 3));
				sut.Subject.Method2(3, 4);
				sut.Subject.Method2(4, 5);
				Exception? result2 = Record.Exception(() => sut.Subject.Method2(5, 6));
				sut.Subject.Method2(6, 7);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo-1-2");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method2(1, 2);

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class VoidMethodWith3Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method3(1, 2, 3);
				Exception? result1 = Record.Exception(() => sut.Subject.Method3(2, 3, 4));
				sut.Subject.Method3(3, 4, 5);
				sut.Subject.Method3(4, 5, 6);
				Exception? result2 = Record.Exception(() => sut.Subject.Method3(5, 6, 7));
				sut.Subject.Method3(6, 7, 8);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method3(1, 2, 3);

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class VoidMethodWith4Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method4(1, 2, 3, 4);
				Exception? result1 = Record.Exception(() => sut.Subject.Method4(2, 3, 4, 5));
				sut.Subject.Method4(3, 4, 5, 6);
				sut.Subject.Method4(4, 5, 6, 7);
				Exception? result2 = Record.Exception(() => sut.Subject.Method4(5, 6, 7, 8));
				sut.Subject.Method4(6, 7, 8, 9);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method4(1, 2, 3, 4);

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}

		public class VoidMethodWith5Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Subject.Method5(1, 2, 3, 4, 5);
				Exception? result1 = Record.Exception(() => sut.Subject.Method5(2, 3, 4, 5, 6));
				sut.Subject.Method5(3, 4, 5, 6, 7);
				sut.Subject.Method5(4, 5, 6, 7, 8);
				Exception? result2 = Record.Exception(() => sut.Subject.Method5(5, 6, 7, 8, 9));
				sut.Subject.Method5(6, 7, 8, 9, 10);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Throws_Callback_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws(() => new Exception("foo"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_ShouldReturnExpectedValue()
			{
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Throws(new Exception("foo"));

				void Act()
					=> sut.Subject.Method5(1, 2, 3, 4, 5);

				await That(Act).ThrowsException().WithMessage("foo");
			}
		}
	}
}
