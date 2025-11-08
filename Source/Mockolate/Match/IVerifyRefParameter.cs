namespace Mockolate.Match;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Matches any <see langword="ref" /> parameter.
/// </summary>
public interface IVerifyRefParameter<out T> : IParameter
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
