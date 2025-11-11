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

		sut.SaveChanges();

		sut.VerifyMock.Invoked.SaveChanges().Once();
	}
}
