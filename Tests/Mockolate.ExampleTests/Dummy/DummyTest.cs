using Mockolate.Tests.Dummy;
using Mockolate.Verify;

namespace Mockolate.ExampleTests.Dummy;

public class DummyTest
{
	[Fact]
	public async Task X()
	{
		IExampleRepository sut = new MockForIExampleRepository(MockBehavior.Default);
		sut.SetupMock.Method.SaveChanges().Returns(true);

		var result = sut.SaveChanges();

		sut.VerifyMock.Invoked.SaveChanges().Once();
		await That(result).IsTrue();
	}
}
