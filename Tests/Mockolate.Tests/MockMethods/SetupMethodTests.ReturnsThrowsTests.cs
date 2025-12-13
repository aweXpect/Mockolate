using System.Collections.Generic;
using Mockolate.Setup;

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method0();
				Exception? result2 = Record.Exception(() => sut.Method0());
				string result3 = sut.Method0();

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("d")
					.Returns("c")
					.Returns(() => "b");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method0();
				}

				await That(result).IsEqualTo(["d", "c", "b", "d", "c", "b", "d", "c", "b", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Returns(() => "d");

				string result = sut.Method0();

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method0());
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method0();
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Returns("d");

				string result = sut.Method0();

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method0();
				string result2 = sut.Method0();
				string result3 = sut.Method0();

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method0());
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method0();

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws(new Exception("foo"));

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string> setup =
					(IReturnMethodSetupReturnBuilder<string>)mock.SetupMock.Method.Method0();

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string> setup =
					(IReturnMethodSetupReturnWhenBuilder<string>)mock.SetupMock.Method.Method0();

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith1Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method1(1);
				Exception? result2 = Record.Exception(() => sut.Method1(2));
				string result3 = sut.Method1(3);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns(v => $"foo-{v}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method1(i);
				}

				await That(result).IsEqualTo(["d", "c", "foo-2", "d", "c", "foo-5", "d", "c", "foo-8", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>()).Returns(() => "d");

				string result = sut.Method1(3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>()).Returns(x => $"foo-{x}");

				string result = sut.Method1(3);

				await That(result).IsEqualTo("foo-3");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method1(1));
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method1(i);
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>()).Returns("d");

				string result = sut.Method1(3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method1(1);
				string result2 = sut.Method1(1);
				string result3 = sut.Method1(1);

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method1(1));
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method1(3);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(v1 => new Exception("foo-" + v1));

				void Act()
				{
					sut.Method1(42);
				}

				await That(Act).ThrowsException().WithMessage("foo-42");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Method1(1);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string, int> setup =
					(IReturnMethodSetupReturnBuilder<string, int>)mock.SetupMock.Method.Method1(It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string, int> setup =
					(IReturnMethodSetupReturnWhenBuilder<string, int>)mock.SetupMock.Method.Method1(It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith2Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method2(1, 2);
				Exception? result2 = Record.Exception(() => sut.Method2(2, 3));
				string result3 = sut.Method2(3, 4);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2) => $"foo-{v1}-{v2}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method2(i, 2 * i);
				}

				await That(result).IsEqualTo(["d", "c", "foo-2-4", "d", "c", "foo-5-10", "d", "c", "foo-8-16", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).Returns(() => "d");

				string result = sut.Method2(2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).Returns((x, y) => $"foo-{x}-{y}");

				string result = sut.Method2(2, 3);

				await That(result).IsEqualTo("foo-2-3");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method2(1, 2));
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method2(i, 2);
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>()).Returns("d");

				string result = sut.Method2(2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method2(1, 2);
				string result2 = sut.Method2(1, 2);
				string result3 = sut.Method2(1, 2);

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method2(1, 2));
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method2(2, 3);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Method2(1, 2);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string, int, int> setup =
					(IReturnMethodSetupReturnBuilder<string, int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string, int, int> setup =
					(IReturnMethodSetupReturnWhenBuilder<string, int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith3Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method3(1, 2, 3);
				Exception? result2 = Record.Exception(() => sut.Method3(2, 3, 4));
				string result3 = sut.Method3(3, 4, 5);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method3(i, 2 * i, 3 * i);
				}

				await That(result)
					.IsEqualTo(["d", "c", "foo-2-4-6", "d", "c", "foo-5-10-15", "d", "c", "foo-8-16-24", "d",]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns(() => "d");

				string result = sut.Method3(1, 2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns((x, y, z) => $"foo-{x}-{y}-{z}");

				string result = sut.Method3(2, 3, 4);

				await That(result).IsEqualTo("foo-2-3-4");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method3(1, 2, 3));
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method3(i, 2, 3);
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d");

				string result = sut.Method3(1, 2, 3);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method3(1, 2, 3);
				string result2 = sut.Method3(1, 2, 3);
				string result3 = sut.Method3(1, 2, 3);

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method3(1, 2, 3));
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method3(2, 3, 4);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string, int, int, int> setup =
					(IReturnMethodSetupReturnBuilder<string, int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string, int, int, int> setup =
					(IReturnMethodSetupReturnWhenBuilder<string, int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith4Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method4(1, 2, 3, 4);
				Exception? result2 = Record.Exception(() => sut.Method4(2, 3, 4, 5));
				string result3 = sut.Method4(3, 4, 5, 6);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3, v4) => $"foo-{v1}-{v2}-{v3}-{v4}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(result).IsEqualTo([
					"d", "c", "foo-2-4-6-8", "d", "c", "foo-5-10-15-20", "d", "c", "foo-8-16-24-32", "d",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns(() => "d");

				string result = sut.Method4(1, 2, 3, 4);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns((x, y, z, a) => $"foo-{x}-{y}-{z}-{a}");

				string result = sut.Method4(2, 3, 4, 5);

				await That(result).IsEqualTo("foo-2-3-4-5");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method4(1, 2, 3, 4));
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method4(i, 2, 3, 4);
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("d");

				string result = sut.Method4(1, 2, 3, 4);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method4(1, 2, 3, 4);
				string result2 = sut.Method4(1, 2, 3, 4);
				string result3 = sut.Method4(1, 2, 3, 4);

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method4(1, 2, 3, 4));
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method4(2, 3, 4, 5);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string, int, int, int, int> setup =
					(IReturnMethodSetupReturnBuilder<string, int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string, int, int, int, int> setup =
					(IReturnMethodSetupReturnWhenBuilder<string, int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith5Parameters
		{
			[Fact]
			public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("d")
					.Throws(new Exception("foo"))
					.Returns(() => "b");

				string result1 = sut.Method5(1, 2, 3, 4, 5);
				Exception? result2 = Record.Exception(() => sut.Method5(2, 3, 4, 5, 6));
				string result3 = sut.Method5(3, 4, 5, 6, 7);

				await That(result1).IsEqualTo("d");
				await That(result2).HasMessage("foo");
				await That(result3).IsEqualTo("b");
			}

			[Fact]
			public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("d")
					.Returns(() => "c")
					.Returns((v1, v2, v3, v4, v5) => $"foo-{v1}-{v2}-{v3}-{v4}-{v5}");

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(result).IsEqualTo([
					"d", "c", "foo-2-4-6-8-10", "d", "c", "foo-5-10-15-20-25", "d", "c", "foo-8-16-24-32-40", "d",
				]);
			}

			[Fact]
			public async Task Returns_Callback_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns(() => "d");

				string result = sut.Method5(1, 2, 3, 4, 5);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns((x, y, z, a, b) => $"foo-{x}-{y}-{z}-{a}-{b}");

				string result = sut.Method5(2, 3, 4, 5, 6);

				await That(result).IsEqualTo("foo-2-3-4-5-6");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("foo").For(2)
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method5(1, 2, 3, 4, 5));
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("a")
					.Returns("b")
					.Returns("c").Forever();

				string[] result = new string[10];
				for (int i = 0; i < 10; i++)
				{
					result[i] = sut.Method5(i, 2, 3, 4, 5);
				}

				await That(result).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_ShouldReturnExpectedValue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("d");

				string result = sut.Method5(1, 2, 3, 4, 5);

				await That(result).IsEqualTo("d");
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("foo").When(i => i > 0);

				string result1 = sut.Method5(1, 2, 3, 4, 5);
				string result2 = sut.Method5(1, 2, 3, 4, 5);
				string result3 = sut.Method5(1, 2, 3, 4, 5);

				await That(result1).IsEqualTo("");
				await That(result2).IsEqualTo("foo");
				await That(result3).IsEqualTo("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Returns("foo").When(i => i > 0).For(2)
					.Returns("baz")
					.Returns("bar").For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					values.Add(sut.Method5(1, 2, 3, 4, 5));
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Returns_WithoutSetup_ShouldReturnDefault()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				string result = sut.Method5(2, 3, 4, 5, 6);

				await That(result).IsEmpty();
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns((string?)null!);

				string result = sut.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(1);
				await That(result).IsNull();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnBuilder<string, int, int, int, int, int> setup =
					(IReturnMethodSetupReturnBuilder<string, int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(),
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IReturnMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupReturnWhenBuilder<string, int, int, int, int, int> setup =
					(IReturnMethodSetupReturnWhenBuilder<string, int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method0();
				Exception? result1 = Record.Exception(() => sut.Method0());
				sut.Method0();
				sut.Method0();
				Exception? result2 = Record.Exception(() => sut.Method0());
				sut.Method0();

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method0();
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method0();
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method0()).DoesNotThrow();
				await That(() => sut.Method0()).ThrowsException().WithMessage("foo");
				await That(() => sut.Method0()).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method0();
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0().Throws(new Exception("foo"));

				void Act()
				{
					sut.Method0();
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder setup =
					(IVoidMethodSetupReturnBuilder)mock.SetupMock.Method.Method0();

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder setup =
					(IVoidMethodSetupReturnWhenBuilder)mock.SetupMock.Method.Method0();

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method1(1);
				Exception? result1 = Record.Exception(() => sut.Method1(2));
				sut.Method1(3);
				sut.Method1(4);
				Exception? result2 = Record.Exception(() => sut.Method1(5));
				sut.Method1(6);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method1(1);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method1(i);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method1(1)).DoesNotThrow();
				await That(() => sut.Method1(1)).ThrowsException().WithMessage("foo");
				await That(() => sut.Method1(1)).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method1(1);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>()).Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>()).Throws(v1 => new Exception($"foo-{v1}"));

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsException().WithMessage("foo-1");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method1(1);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder<int> setup =
					(IVoidMethodSetupReturnBuilder<int>)mock.SetupMock.Method.Method1(It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder<int> setup =
					(IVoidMethodSetupReturnWhenBuilder<int>)mock.SetupMock.Method.Method1(It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith2Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method2(1, 2);
				Exception? result1 = Record.Exception(() => sut.Method2(2, 3));
				sut.Method2(3, 4);
				sut.Method2(4, 5);
				Exception? result2 = Record.Exception(() => sut.Method2(5, 6));
				sut.Method2(6, 7);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method2(1, 2);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method2(i, 2);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method2(1, 2)).DoesNotThrow();
				await That(() => sut.Method2(1, 2)).ThrowsException().WithMessage("foo");
				await That(() => sut.Method2(1, 2)).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method2(1, 2);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method2(1, 2);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder<int, int> setup =
					(IVoidMethodSetupReturnBuilder<int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder<int, int> setup =
					(IVoidMethodSetupReturnWhenBuilder<int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith3Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method3(1, 2, 3);
				Exception? result1 = Record.Exception(() => sut.Method3(2, 3, 4));
				sut.Method3(3, 4, 5);
				sut.Method3(4, 5, 6);
				Exception? result2 = Record.Exception(() => sut.Method3(5, 6, 7));
				sut.Method3(6, 7, 8);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method3(1, 2, 3);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method3(i, 2, 3);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method3(1, 2, 3)).DoesNotThrow();
				await That(() => sut.Method3(1, 2, 3)).ThrowsException().WithMessage("foo");
				await That(() => sut.Method3(1, 2, 3)).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method3(1, 2, 3);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method3(1, 2, 3);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder<int, int, int> setup =
					(IVoidMethodSetupReturnBuilder<int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder<int, int, int> setup =
					(IVoidMethodSetupReturnWhenBuilder<int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith4Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method4(1, 2, 3, 4);
				Exception? result1 = Record.Exception(() => sut.Method4(2, 3, 4, 5));
				sut.Method4(3, 4, 5, 6);
				sut.Method4(4, 5, 6, 7);
				Exception? result2 = Record.Exception(() => sut.Method4(5, 6, 7, 8));
				sut.Method4(6, 7, 8, 9);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method4(1, 2, 3, 4);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method4(i, 2, 3, 4);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method4(1, 2, 3, 4)).DoesNotThrow();
				await That(() => sut.Method4(1, 2, 3, 4)).ThrowsException().WithMessage("foo");
				await That(() => sut.Method4(1, 2, 3, 4)).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method4(1, 2, 3, 4);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method4(1, 2, 3, 4);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder<int, int, int, int> setup =
					(IVoidMethodSetupReturnBuilder<int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder<int, int, int, int> setup =
					(IVoidMethodSetupReturnWhenBuilder<int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith5Parameters
		{
			[Fact]
			public async Task MixDoesNotThrowAndThrow_ShouldIterateThroughBoth()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.DoesNotThrow()
					.Throws(new Exception("foo"))
					.DoesNotThrow();

				sut.Method5(1, 2, 3, 4, 5);
				Exception? result1 = Record.Exception(() => sut.Method5(2, 3, 4, 5, 6));
				sut.Method5(3, 4, 5, 6, 7);
				sut.Method5(4, 5, 6, 7, 8);
				Exception? result2 = Record.Exception(() => sut.Method5(5, 6, 7, 8, 9));
				sut.Method5(6, 7, 8, 9, 10);

				await That(result1).HasMessage("foo");
				await That(result2).HasMessage("foo");
			}

			[Fact]
			public async Task Returns_For_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("foo")).For(2)
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method5(1, 2, 3, 4, 5);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["foo", "foo", "bar", "bar", "bar", "", "", "", "", "",]);
			}

			[Fact]
			public async Task Returns_Forever_ShouldUseTheLastValueForever()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("a"))
					.Throws(new Exception("b"))
					.Throws(new Exception("c")).Forever();

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method5(i, 2, 3, 4, 5);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["a", "b", "c", "c", "c", "c", "c", "c", "c", "c",]);
			}

			[Fact]
			public async Task Returns_When_ShouldOnlyUseValueWhenPredicateIsTrue()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0);

				await That(() => sut.Method5(1, 2, 3, 4, 5)).DoesNotThrow();
				await That(() => sut.Method5(1, 2, 3, 4, 5)).ThrowsException().WithMessage("foo");
				await That(() => sut.Method5(1, 2, 3, 4, 5)).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Returns_WhenFor_ShouldLimitUsage_ToSpecifiedNumber()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("foo")).When(i => i > 0).For(2)
					.Throws(new Exception("baz"))
					.Throws(new Exception("bar")).For(3);

				List<string> values = [];
				for (int i = 0; i < 10; i++)
				{
					try
					{
						sut.Method5(1, 2, 3, 4, 5);
						values.Add("");
					}
					catch (Exception ex)
					{
						values.Add(ex.Message);
					}
				}

				await That(values).IsEqualTo(["baz", "bar", "bar", "bar", "foo", "foo", "baz", "baz", "baz", "baz",]);
			}

			[Fact]
			public async Task Throws_Callback_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(() => new Exception("foo"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task Throws_CallbackWithValue_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
			}

			[Fact]
			public async Task Throws_Generic_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws<ArgumentNullException>();

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsExactly<ArgumentNullException>();
			}

			[Fact]
			public async Task Throws_ShouldThrowException()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Throws(new Exception("foo"));

				void Act()
				{
					sut.Method5(1, 2, 3, 4, 5);
				}

				await That(Act).ThrowsException().WithMessage("foo");
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnBuilder<int, int, int, int, int> setup =
					(IVoidMethodSetupReturnBuilder<int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(),
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupReturnWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupReturnWhenBuilder<int, int, int, int, int> setup =
					(IVoidMethodSetupReturnWhenBuilder<int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}
	}
}
