using Mockolate.Exceptions;
using Mockolate.Setup;
using static Mockolate.Tests.Setup.MockSetupsTests.MethodsTests;

namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class PropertiesTests
	{
		[Fact]
		public async Task ShouldStoreLastValue()
		{
			var mock = Mock.Create<IPropertyService>();
			IMock sut = mock;

			var result0 = sut.Get<string>("my.property");
			sut.Set("my.property", "foo");
			var result1 = sut.Get<string>("my.property");
			var result2 = sut.Get<string>("my.other.property");

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
		{
			var mock = Mock.Create<IPropertyService>();
			IMockSetup setup = mock.Setup;
			IMock sut = mock;

			var result0 = sut.Get<int>("my.other.property");
			setup.RegisterProperty("my.property", new PropertySetup<int>().InitializeWith(42));
			var result1 = sut.Get<int>("my.property");

			await That(result0).IsEqualTo(0);
			await That(result1).IsEqualTo(42);
		}

		[Fact]
		public async Task Register_SamePropertyTwice_ShouldThrowMockException()
		{
			var mock = Mock.Create<IPropertyService>();
			IMockSetup setup = mock.Setup;
			IMock sut = mock;

			setup.RegisterProperty("my.property", new PropertySetup<int>());
			void Act()
				=> setup.RegisterProperty("my.property", new PropertySetup<int>());

			await That(Act).Throws<MockException>()
				.WithMessage($"You cannot setup property 'my.property' twice.");

			setup.RegisterProperty("my.other.property", new PropertySetup<int>());
		}

		public interface IPropertyService
		{
			string MyProperty { get; set; }
		}
	}
}
