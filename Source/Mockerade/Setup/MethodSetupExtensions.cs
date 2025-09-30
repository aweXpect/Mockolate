namespace Mockerade.Setup;

internal static class MethodSetupExtensions
{
	public static bool TryCast<T>(this object? value, out T result)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}
		//TODO: Use behavior?
		result = default!;
		return value is null;
	}
}
