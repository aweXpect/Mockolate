namespace Mockolate.Match;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Matches any <see langword="out" /> parameter.
/// </summary>
public interface IVerifyOutParameter<out T> : IParameter
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
