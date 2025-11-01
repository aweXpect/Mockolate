using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public class MockGeneratorAttributeTests
{
	[Fact]
	public async Task WithCustomGenerator_ShouldCreateMock()
	{
		Mock<IFoo> mock = MyGenerator.MyCreator<IFoo>();
		mock.Setup.Method.Bar().Returns(42);

		int result = mock.Subject.Bar();

		await That(result).IsEqualTo(42);
		await That(mock.Verify.Invoked.Bar()).Once();
	}

	public interface IFoo
	{
		int Bar();
	}
}
