namespace Mockolate.Verify;

/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="T" />.
/// </summary>
public interface IMockVerify<out T> : IMockVerifyVerb<T>
{
}

public interface IMockVerifyVerb<out T>
{
	
}
#pragma warning restore S2326 // Unused type parameters should be removed
