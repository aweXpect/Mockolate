namespace Mockolate.AotCompatibility.TestApp;

internal static class Program
{
	private static void Main(string[] args)
	{
		IMyInterface sut = IMyInterface.CreateMock();
		sut.Mock.Setup.MyMethod().Returns(2);

		int result = sut.MyMethod();

		Console.WriteLine($"The mock returned: {result}");
	}
}
