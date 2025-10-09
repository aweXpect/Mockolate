using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class PropertiesTests
	{
		[Fact]
		public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
		{
			Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
			IMockSetup setup = mock.Setup;
			IMock sut = mock;

			int result0 = sut.Get<int>("my.other.property");
			setup.RegisterProperty("my.property", new PropertySetup<int>().InitializeWith(42));
			int result1 = sut.Get<int>("my.property");

			await That(result0).IsEqualTo(0);
			await That(result1).IsEqualTo(42);
		}

		[Fact]
		public async Task Register_SamePropertyTwice_ShouldThrowMockException()
		{
			Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
			IMockSetup setup = mock.Setup;
			IMock sut = mock;

			setup.RegisterProperty("my.property", new PropertySetup<int>());

			void Act()
				=> setup.RegisterProperty("my.property", new PropertySetup<int>());

			await That(Act).Throws<MockException>()
				.WithMessage("You cannot setup property 'my.property' twice.");

			setup.RegisterProperty("my.other.property", new PropertySetup<int>());
		}

		[Fact]
		public async Task ShouldStoreLastValue()
		{
			Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
			IMock sut = mock;

			string result0 = sut.Get<string>("my.property");
			sut.Set("my.property", "foo");
			string result1 = sut.Get<string>("my.property");
			string result2 = sut.Get<string>("my.other.property");

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		public interface IPropertyService
		{
			string MyProperty { get; set; }
		}
	}
}
