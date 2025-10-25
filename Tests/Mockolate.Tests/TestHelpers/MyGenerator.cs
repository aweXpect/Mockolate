namespace Mockolate.Tests.TestHelpers;

public static class MyGenerator
{
	[MockGenerator]
	public static Mock<T> MyCreator<T>()
		where T : class
	{
		return Mock.Create<T>();
	}
}
