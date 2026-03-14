namespace Mockolate.AotCompatibility.TestApp;

internal class Program
{
	private static void Main(string[] args)
	{
		IMyInterface mock = IMyInterface.CreateMock();
		mock.Mock.Setup.MyMethod().Returns(2);

		int result = mock.MyMethod();

		Console.WriteLine($"The mock returned: {result}");
	}
}
