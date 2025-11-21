namespace Mockolate.Tests.TestHelpers;

internal static class Record
{
	public static Exception? Exception(Action action)
	{
		try
		{
			action();
			return null;
		}
		catch (Exception exception)
		{
			return exception;
		}
	}
}
