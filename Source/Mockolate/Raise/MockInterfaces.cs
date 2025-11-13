using Mockolate.Setup;

namespace Mockolate.Raise;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Provides methods for managing events on a mock object, including raising events and associating or dissociating
///     event handlers.
/// </summary>
public interface IMockRaises<out T> : IInteractiveMock<T>;

/// <summary>
///     Provides methods for managing events on a mock object, including raising events and associating or dissociating
///     event handlers.
/// </summary>
public interface IProtectedMockRaises<out T> : IInteractiveMock<T>;
#pragma warning restore S2326 // Unused type parameters should be removed
