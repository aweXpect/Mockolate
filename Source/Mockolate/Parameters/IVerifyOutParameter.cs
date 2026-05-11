namespace Mockolate.Parameters;

#pragma warning disable S2326 // Unused type parameters should be removed
// ReSharper disable once UnusedTypeParameter
/// <summary>
///     Matches any <see langword="out" /> parameter.
/// </summary>
public interface IVerifyOutParameter<out T>
#if NET9_0_OR_GREATER
	where T : allows ref struct
#endif
;
#pragma warning restore S2326 // Unused type parameters should be removed
