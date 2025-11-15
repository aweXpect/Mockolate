namespace Mockolate.AotCompatibility.TestApp;

internal class Program
{
	private static void Main(string[] args)
	{
		IMyInterface mock = Mock.Create<IMyInterface>();
		mock.SetupMock.Method.MyMethod().Returns(2);

		int result = mock.MyMethod();

		Console.WriteLine($"The mock returned: {result}");
	}

	public interface IMyInterface
	{
		int MyMethod();
	}
}
