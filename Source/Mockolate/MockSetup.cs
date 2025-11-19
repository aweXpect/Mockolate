using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S1939 // Inheritance list should not be redundant
// ReSharper disable RedundantExtendsListEntry
/// <summary>
///     A partial mock for setting up type <typeparamref name="T" />.
/// </summary>
public class MockSetup<T> : IHasMockRegistration, IMockSetup<T>, IProtectedMockSetup<T>,
	IMockMethodSetup<T>, IProtectedMockMethodSetup<T>,
	IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEquals<T>, IMockMethodSetupWithToStringWithGetHashCode<T>,
	IMockMethodSetupWithEqualsWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEqualsWithGetHashCode<T>,
	IMockPropertySetup<T>, IProtectedMockPropertySetup<T>
{
	/// <inheritdoc cref="Mock{T}" />
	public MockSetup(MockRegistration mockRegistration)
	{
		Registrations = mockRegistration;
		Interactions = mockRegistration.Interactions;
	}

	/// <summary>
	///     The registered interactions on the mock.
	/// </summary>
	public MockInteractions Interactions { get; }

	/// <inheritdoc cref="IHasMockRegistration.Registrations" />
	public MockRegistration Registrations { get; }

	/// <inheritdoc cref="IMockMethodSetupWithEquals{T}.Equals(Match.IParameter{object?})" />
	ReturnMethodSetup<bool, object?> IMockMethodSetupWithEquals<T>.Equals(Match.IParameter<object?> obj)
	{
		ReturnMethodSetup<bool, object?> methodSetup =
			new(Registrations.Prefix + ".Equals", new Match.NamedParameter("obj", (Match.IParameter)obj));
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}

	/// <inheritdoc cref="IMockMethodSetupWithGetHashCode{T}.GetHashCode()" />
	ReturnMethodSetup<int> IMockMethodSetupWithGetHashCode<T>.GetHashCode()
	{
		ReturnMethodSetup<int> methodSetup = new(Registrations.Prefix + ".GetHashCode");
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}

	/// <inheritdoc cref="IMockMethodSetupWithToString{T}.ToString()" />
	ReturnMethodSetup<string> IMockMethodSetupWithToString<T>.ToString()
	{
		ReturnMethodSetup<string> methodSetup = new(Registrations.Prefix + ".ToString");
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}
}
#pragma warning restore S1939 // Inheritance list should not be redundant
