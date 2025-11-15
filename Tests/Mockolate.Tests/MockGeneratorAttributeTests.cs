using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public class MockGeneratorAttributeTests
{
	[Fact]
	public async Task WithCustomGenerator_ShouldCreateMock()
	{
		IFoo mock = MyGenerator.MyCreator<IFoo>();
		mock.SetupMock.Method.Bar().Returns(42);

		int result = mock.Bar();

		await That(result).IsEqualTo(42);
		await That(mock.VerifyMock.Invoked.Bar()).Once();
	}

	public interface IFoo
	{
		int Bar();
	}
}
