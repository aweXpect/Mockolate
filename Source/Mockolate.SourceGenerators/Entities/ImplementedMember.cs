namespace Mockolate.SourceGenerators.Entities;

/// <summary>
///     A member of an additional interface paired with the member of the mocked class that the compiler
///     resolved as its implementation.
/// </summary>
/// <remarks>
///     Both sides are built from the Roslyn symbols, so the pairing is the compiler's own answer to
///     "which class member fills this interface slot" rather than a signature match. See
///     <see cref="MockClass.FindImplementation(Method)" /> and <see cref="Class.RebaseOnto" />.
/// </remarks>
/// <typeparam name="TMember">
///     The member entity: <see cref="Method" />, <see cref="Property" /> or <see cref="Event" />.
/// </typeparam>
/// <param name="InterfaceMember">The member as declared on the interface.</param>
/// <param name="ClassMember">The member of the mocked class that implements it.</param>
internal sealed record ImplementedMember<TMember>(TMember InterfaceMember, TMember ClassMember)
	where TMember : notnull;
