namespace Mockolate.V2;

public interface IMock
{
	MockBehavior Behavior { get; }
}

public interface IHasMockRegistration
{
	IMockRegistration Registrations { get; }
}
