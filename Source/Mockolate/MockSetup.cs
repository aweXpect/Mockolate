using System;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
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
	/// <summary>
	///     This constructor should not be used by user code. It is only intended for use within the mock registration logic
	///     to create an intermediate setup instance when the subject is not yet available.
	/// </summary>
	/// <remarks>
	///     Create a <see cref="Mock{T}" /> instead.
	/// </remarks>
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This constructor should not be used by user code. Create a `Mock<T>` instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
	public MockSetup(MockRegistration mockRegistration)
	{
		Registrations = mockRegistration;
		Interactions = mockRegistration.Interactions;
	}

	/// <inheritdoc cref="MockSetup{T}" />
	protected MockSetup(T subject, MockRegistration mockRegistration)
	{
		Subject = subject;
		Registrations = mockRegistration;
		Interactions = mockRegistration.Interactions;
	}

	/// <summary>
	///     The registered interactions on the mock.
	/// </summary>
	public MockInteractions Interactions { get; }

	/// <inheritdoc cref="IHasMockRegistration.Registrations" />
	public MockRegistration Registrations { get; }

	/// <inheritdoc cref="IMockMethodSetupWithEquals{T}.Equals(IParameter{object?})" />
	ReturnMethodSetup<bool, object?> IMockMethodSetupWithEquals<T>.Equals(IParameter<object?> obj)
	{
		ReturnMethodSetup<bool, object?> methodSetup =
			new(Registrations.Prefix + ".Equals", new NamedParameter("obj", (IParameter)obj));
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

	/// <summary>
	///     The mock subject.
	/// </summary>
	public T Subject
		=> field
		   ?? throw new MockException("Subject is not yet available. You can only access the subject in callbacks!");
}
#pragma warning restore S1939 // Inheritance list should not be redundant
