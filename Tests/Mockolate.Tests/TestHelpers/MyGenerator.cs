namespace Mockolate.Tests.TestHelpers;

public static class MyGenerator
{
	[MockGenerator]
	public static T MyCreator<T>()
		where T : class
		=> Mock.Create<T>();
}
