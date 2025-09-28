using Mockerade.Setup;

namespace Mockerade.Tests.TestHelpers;

public class MyMock<T>(T @object) : Mock<T>(MockBehavior.Default)
{
	public IMockSetup HiddenSetup => Setup;
	public IMock Hidden => this;
	public override T Object => @object;
}
