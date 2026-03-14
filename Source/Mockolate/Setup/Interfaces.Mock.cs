namespace Mockolate.Setup;

/// <summary>
///     Marker interface for setups.
/// </summary>
public interface ISetup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockSetup<out T> : IInteractiveMock<T>;
#pragma warning restore S2326 // Unused type parameters should be removed
