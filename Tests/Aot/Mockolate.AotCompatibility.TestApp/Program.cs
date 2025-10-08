using Mockolate;

namespace Mockolate.AotCompatibility.TestApp;

internal class Program
{
	static void Main(string[] args)
	{
		Mock<IMyInterface> mock = Mock.Create<IMyInterface>();
		mock.Setup.MyMethod().Returns(2);

		int result = mock.Object.MyMethod();

		Console.WriteLine($"The mock returned: {result}");
	}

	public interface IMyInterface
	{
		int MyMethod();
	}
}
