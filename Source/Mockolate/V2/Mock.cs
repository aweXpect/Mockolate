using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.V2;

public partial class Mock<T> : IHasMockRegistration, IMockSetup<T>, IMockRaises<T>, IProtectedMockSetup<T>,
	IMockMethodSetup<T>, IProtectedMockMethodSetup<T>,
	IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEquals<T>, IMockMethodSetupWithToStringWithGetHashCode<T>,IMockMethodSetupWithEqualsWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEqualsWithGetHashCode<T>,
	IMockPropertySetup<T>, IProtectedMockPropertySetup<T>
{
	public IMockRegistration Registrations { get; }
	
	public Mock(T subject, IMockRegistration mockRegistration)
	{
		Subject = subject;
		Registrations = mockRegistration;
	}
	ReturnMethodSetup<int> IMockMethodSetupWithGetHashCode<T>.GetHashCode()
	{
		var methodSetup = new ReturnMethodSetup<int>(Registrations.Prefix + ".GetHashCode");
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}

	/// <inheritdoc cref="IMockMethodSetupWithToString{T}.ToString()" />
	ReturnMethodSetup<string> IMockMethodSetupWithToString<T>.ToString()
	{
		var methodSetup = new ReturnMethodSetup<string>(Registrations.Prefix + ".ToString");
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}

	ReturnMethodSetup<bool, object?> IMockMethodSetupWithEquals<T>.Equals(Match.IParameter<object?> obj)
	{
		var methodSetup = new ReturnMethodSetup<bool, object?>(Registrations.Prefix + ".Equals", new Match.NamedParameter("obj", obj));
		Registrations.SetupMethod(methodSetup);
		return methodSetup;
	}

	public T Subject { get; }
	public MockInteractions Interactions => Registrations.Interactions;
}
