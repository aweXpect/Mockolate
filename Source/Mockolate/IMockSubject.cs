namespace Mockolate;

public interface IMockSubject<T>
{
	V2.Mock<T> Mock { get; }
}
