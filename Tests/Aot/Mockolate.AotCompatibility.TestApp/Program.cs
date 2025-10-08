using Mockolate;

namespace Mockolate.AotCompatibility.TestApp;

internal class Program
{
	static void Main(string[] args)
	{
		var mock = Mock.Create<IMyInterface>();
	}

	public interface IMyInterface
	{
		void MyMethod();
	}
}
